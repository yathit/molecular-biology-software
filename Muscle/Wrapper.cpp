#include <stdlib.h>
#include <string>
#include <vector>
#include "wrapper.h"
#include "muscle.h"
#include "textfile.h"
#include "seqvect.h"
#include "distfunc.h"
#include "msa.h"
#include "tree.h"
#include "profile.h"
#include "timing.h"

#pragma once


using namespace std;
using namespace System;
using namespace System::Text;
using namespace System::Collections::Generic;
using namespace System::IO;

namespace Muscle
{
	
	void MarshalString ( String ^ s, string& os ) {
		using namespace Runtime::InteropServices;
		const char* chars = 
			(const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
		os = chars;
		Marshal::FreeHGlobal(IntPtr((void*)chars));
	}

	void MarshalString ( String ^ s, wstring& os ) {
		using namespace Runtime::InteropServices;
		const wchar_t* chars = 
			(const wchar_t*)(Marshal::StringToHGlobalUni(s)).ToPointer();
		os = chars;
		Marshal::FreeHGlobal(IntPtr((void*)chars));
	}


	bool Multialign::DoMuscle(List<String ^>^ seqs, List<String ^>^ ids, 
			List<String ^>^ alns)
	{
		vector<string> sSeqs;
		vector<string> sIds;

		string seq;
		string id;
		for (int i = 0; i < seqs->Count; i++)
		{
			MarshalString(seqs[i], seq);
			MarshalString(ids[i], id);
			sSeqs.push_back(seq);
			sIds.push_back(id);
		}

		_doMuscle(sSeqs, sIds, alns);

		

		return 0;
	}
	

	void Multialign::_doMuscle(vector<string> seqs, vector<string> ids, List<String ^>^ alns)
	{
		// SetOutputFileName(g_pstrOutFileName);
		// SetInputFileName(g_pstrInFileName);

		SetMaxIters(g_uMaxIters);
		SetSeqWeightMethod(g_SeqWeight1);

		// TextFile fileIn(g_pstrInFileName);
		SeqVect v;
		// v.FromFASTAFile(fileIn);
		for (int i = 0; i < ids.size(); i++)
		{
			Seq *ptrSeq = new Seq;
			ptrSeq->SetId(i);
			ptrSeq->FromString(seqs.at(i).c_str(), ids.at(i).c_str());
			v.AppendSeq(*ptrSeq);
		}
		

		const unsigned uSeqCount = v.Length();

		if (0 == uSeqCount)
			Quit("No sequences in input file");

		ALPHA Alpha = ALPHA_Undefined;
		switch (g_SeqType)
		{
		case SEQTYPE_Auto:
			Alpha = v.GuessAlpha();
			break;

		case SEQTYPE_Protein:
			Alpha = ALPHA_Amino;
			break;

		case SEQTYPE_DNA:
			Alpha = ALPHA_DNA;
			break;

		case SEQTYPE_RNA:
			Alpha = ALPHA_RNA;
			break;

		default:
			Quit("Invalid seq type");
		}
		SetAlpha(Alpha);
		v.FixAlpha();

		PTR_SCOREMATRIX UserMatrix = 0;
		if (0 != g_pstrMatrixFileName)
		{
			const char *FileName = g_pstrMatrixFileName;
			const char *Path = getenv("MUSCLE_MXPATH");
			if (Path != 0)
			{
				size_t n = strlen(Path) + 1 + strlen(FileName) + 1;
				char *NewFileName = new char[n];
				sprintf(NewFileName, "%s/%s", Path, FileName);
				FileName = NewFileName;
			}
			TextFile File(FileName);
			UserMatrix = ReadMx(File);
			g_Alpha = ALPHA_Amino;
			g_PPScore = PPSCORE_SP;
		}

		SetPPScore();

		if (0 != UserMatrix)
			g_ptrScoreMatrix = UserMatrix;

		unsigned uMaxL = 0;
		unsigned uTotL = 0;
		for (unsigned uSeqIndex = 0; uSeqIndex < uSeqCount; ++uSeqIndex)
		{
			unsigned L = v.GetSeq(uSeqIndex).Length();
			uTotL += L;
			if (L > uMaxL)
				uMaxL = L;
		}

		SetIter(1);
		g_bDiags = g_bDiags1;
		SetSeqStats(uSeqCount, uMaxL, uTotL/uSeqCount);

		SetMuscleSeqVect(v);

		MSA::SetIdCount(uSeqCount);

		// Initialize sequence ids.
		// From this point on, ids must somehow propogate from here.
		for (unsigned uSeqIndex = 0; uSeqIndex < uSeqCount; ++uSeqIndex)
			v.SetSeqId(uSeqIndex, uSeqIndex);

		if (0 == uSeqCount)
			Quit("Input file '%s' has no sequences", g_pstrInFileName);
		if (1 == uSeqCount)
		{
			TextFile fileOut(g_pstrOutFileName, true);
			v.ToFile(fileOut);
			return;
		}

		if (uSeqCount > 1)
			MHackStart(v);

		// First iteration
		Tree GuideTree;
		if (0 != g_pstrUseTreeFileName)
		{

			// Read tree from file
			TextFile TreeFile(g_pstrUseTreeFileName);
			GuideTree.FromFile(TreeFile);

			// Make sure tree is rooted
			if (!GuideTree.IsRooted())
				Quit("User tree must be rooted");

			if (GuideTree.GetLeafCount() != uSeqCount)
				Quit("User tree does not match input sequences");

			const unsigned uNodeCount = GuideTree.GetNodeCount();
			for (unsigned uNodeIndex = 0; uNodeIndex < uNodeCount; ++uNodeIndex)
			{
				if (!GuideTree.IsLeaf(uNodeIndex))
					continue;
				const char *LeafName = GuideTree.GetLeafName(uNodeIndex);
				unsigned uSeqIndex;
				bool SeqFound = v.FindName(LeafName, &uSeqIndex);
				if (!SeqFound)
					Quit("Label %s in tree does not match sequences", LeafName);
			}

			// Set ids
			for (unsigned uSeqIndex = 0; uSeqIndex < uSeqCount; ++uSeqIndex)
			{
				const char *SeqName = v.GetSeqName(uSeqIndex);
				unsigned uLeafIndex = GuideTree.GetLeafNodeIndex(SeqName);
				GuideTree.SetLeafId(uLeafIndex, uSeqIndex);
			}
		}
		else
			TreeFromSeqVect(v, GuideTree, g_Cluster1, g_Distance1, g_Root1);

		const char *Tree1 = ValueOpt("Tree1");
		if (0 != Tree1)
		{
			TextFile f(Tree1, true);
			GuideTree.ToFile(f);
			if (g_bCluster)
				return;
		}

		SetMuscleTree(GuideTree);
		ValidateMuscleIds(GuideTree);

		MSA msa;
		ProgNode *ProgNodes = 0;
		if (g_bLow)
			ProgNodes = ProgressiveAlignE(v, GuideTree, msa);
		else
			ProgressiveAlign(v, GuideTree, msa);
		SetCurrentAlignment(msa);



		ValidateMuscleIds(msa);

		if (1 == g_uMaxIters || 2 == uSeqCount)
		{
			//TextFile fileOut(g_pstrOutFileName, true);
			//MHackEnd(msa);
			//msa.ToFile(fileOut);
			MuscleOutput(msa);
			return;
		}

		if (0 == g_pstrUseTreeFileName)
		{
			g_bDiags = g_bDiags2;
			SetIter(2);

			if (g_bLow)
			{
				if (0 != g_uMaxTreeRefineIters)
					RefineTreeE(msa, v, GuideTree, ProgNodes);
			}
			else
				RefineTree(msa, GuideTree);

			const char *Tree2 = ValueOpt("Tree2");
			if (0 != Tree2)
			{
				TextFile f(Tree2, true);
				GuideTree.ToFile(f);
			}
		}

		SetSeqWeightMethod(g_SeqWeight2);
		SetMuscleTree(GuideTree);

		if (g_bAnchors)
			RefineVert(msa, GuideTree, g_uMaxIters - 2);
		else
			RefineHoriz(msa, GuideTree, g_uMaxIters - 2, false, false);


		ValidateMuscleIds(msa);
		ValidateMuscleIds(GuideTree);

		
		//char* outFile = "C:\\Users\\kyawtun\\Documents\\out.txt";
		//TextFile fileOut(outFile, true);
		MHackEnd(msa);
		//msa.ToFile(fileOut);
		//MuscleOutput(msa);

		Dictionary<int, System::String^> seqdict = gcnew Dictionary<int, System::String^>();
		const unsigned uColCount = msa.GetColCount();
		for (int i = 0; i < uSeqCount; i++)
		{
			char* chars = new char[uColCount+1];
			for(int j = 0; j < uColCount; j++)
			{
				chars[j] = msa.GetChar(i, j);
				// FIXME: i don't know what is the different between 'X' and '-'
				if (chars[j] == 'X')
				{
					chars[j] = '-';
				}
			}
			chars[uColCount] = 0;
			const unsigned id = msa.GetSeqId(i);
			
			// alns->Insert(id, gcnew System::String(chars));
			seqdict[id] = gcnew System::String(chars);
		}

		for (int i = 0; i < uSeqCount; i++)
		{
			alns->Add(seqdict[i]);
		}
		
	}

}