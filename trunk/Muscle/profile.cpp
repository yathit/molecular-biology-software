#include "muscle.h"
#include "textfile.h"
#include "msa.h"
#include "tree.h"
#include "profile.h"
#include "objscore.h"

static ProfPos *ProfileFromMSALocal(MSA &msa, Tree &tree)
	{
	const unsigned uSeqCount = msa.GetSeqCount();
	for (unsigned uSeqIndex = 0; uSeqIndex < uSeqCount; ++uSeqIndex)
		msa.SetSeqId(uSeqIndex, uSeqIndex);

	TreeFromMSA(msa, tree, g_Cluster2, g_Distance2, g_Root1);
	SetMuscleTree(tree);
	return ProfileFromMSA(msa);
	}

void ProfileProfile(MSA &msa1, MSA &msa2, MSA &msaOut)
	{
	ALPHA Alpha = ALPHA_Undefined;
	switch (g_SeqType)
		{
	case SEQTYPE_Auto:
		Alpha = msa1.GuessAlpha();
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
		Quit("Invalid SeqType");
		}
	SetAlpha(Alpha);

	msa1.FixAlpha();
	msa2.FixAlpha();

	if (ALPHA_DNA == Alpha || ALPHA_RNA == Alpha)
		SetPPScore(PPSCORE_SPN);

	unsigned uLength1;
	unsigned uLength2;

	uLength1 = msa1.GetColCount();
	uLength2 = msa2.GetColCount();

	Tree tree1;
	Tree tree2;
	ProfPos *Prof1 = ProfileFromMSALocal(msa1, tree1);
	ProfPos *Prof2 = ProfileFromMSALocal(msa2, tree2);

	PWPath Path;
	ProfPos *ProfOut;
	unsigned uLengthOut;
	AlignTwoProfs(Prof1, uLength1, 1.0, Prof2, uLength2, 1.0, Path, &ProfOut, &uLengthOut);

	AlignTwoMSAsGivenPath(Path, msa1, msa2, msaOut);
	}

// Do profile-profile alignment
void Profile()
	{
	if (0 == g_pstrFileName1 || 0 == g_pstrFileName2)
		Quit("-profile needs -in1 and -in2");

	SetSeqWeightMethod(g_SeqWeight1);

	TextFile file1(g_pstrFileName1);
	TextFile file2(g_pstrFileName2);

	MSA msa1;
	MSA msa2;
	MSA msaOut;

	msa1.FromFile(file1);
	msa2.FromFile(file2);

	ALPHA Alpha = ALPHA_Undefined;
	switch (g_SeqType)
		{
	case SEQTYPE_Auto:
		Alpha = msa1.GuessAlpha();
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
	msa1.FixAlpha();
	msa2.FixAlpha();
	SetPPScore();

	const unsigned uSeqCount1 = msa1.GetSeqCount();
	const unsigned uSeqCount2 = msa2.GetSeqCount();
	//const unsigned uMaxSeqCount = (uSeqCount1 > uSeqCount2 ? uSeqCount1 : uSeqCount2);
	//MSA::SetIdCount(uMaxSeqCount);
	const unsigned uSumSeqCount = uSeqCount1 + uSeqCount2;
	MSA::SetIdCount(uSumSeqCount);

	//msa1.FromFile(file1);
	//msa2.FromFile(file2);

	//ALPHA Alpha = ALPHA_Undefined;
	//switch (g_SeqType)
	//	{
	//case SEQTYPE_Auto:
	//	Alpha = msa1.GuessAlpha();
	//	break;

	//case SEQTYPE_Protein:
	//	Alpha = ALPHA_Amino;
	//	break;

	//case SEQTYPE_Nucleo:
	//	Alpha = ALPHA_Nucleo;
	//	break;

	//default:
	//	Quit("Invalid SeqType");
	//	}
	//SetAlpha(Alpha);

	//msa1.FixAlpha();
	//msa2.FixAlpha();

	//if (ALPHA_Nucleo == Alpha)
	//	SetPPScore(PPSCORE_SPN);

	//unsigned uLength1;
	//unsigned uLength2;

	//uLength1 = msa1.GetColCount();
	//uLength2 = msa2.GetColCount();

	//const unsigned uSeqCount1 = msa1.GetSeqCount();
	//const unsigned uSeqCount2 = msa2.GetSeqCount();
	//const unsigned uMaxSeqCount = (uSeqCount1 > uSeqCount2 ? uSeqCount1 : uSeqCount2);
	//MSA::SetIdCount(uMaxSeqCount);

	//Tree tree1;
	//Tree tree2;
	//ProfPos *Prof1 = ProfileFromMSALocal(msa1, tree1);
	//ProfPos *Prof2 = ProfileFromMSALocal(msa2, tree2);

	//PWPath Path;
	//ProfPos *ProfOut;
	//unsigned uLengthOut;
	//AlignTwoProfs(Prof1, uLength1, 1.0, Prof2, uLength2, 1.0, Path, &ProfOut, &uLengthOut);

	//MSA msaOut;
	//AlignTwoMSAsGivenPath(Path, msa1, msa2, msaOut);

	ProfileProfile(msa1, msa2, msaOut);

//	TextFile fileOut(g_pstrOutFileName, true);
//	msaOut.ToFile(fileOut);
	MuscleOutput(msaOut);
	}
