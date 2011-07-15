// NcbiWrapper.h

#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::IO;

namespace NcbiWrapper {

	[DllImport("kernel32.dll", CharSet = CharSet::Auto)]
	extern IntPtr LoadLibrary(String ^ lpFileName);

	public ref struct AlignmentResult
	{
		int Score;
		String ^ Alignment;
	};


	public ref class Algo
	{
	public:
		/// <summary>
		/// Needleman-Wunsch alignment of two sequences
		/// </summary>
		/// <param name="idA"></param>
		/// <param name="seqA"></param>
		/// <param name="lenSeqA"></param>
		/// <param name="idB"></param>
		/// <param name="seqB"></param>
		/// <param name="lenSeqB"></param>
		/// <returns>alignment result</returns>
		static AlignmentResult ^ Nwalign(String ^ idA, String ^ seqA, 
			String ^ idB, String ^ seqB);
		static Algo(){
			String ^ basePath = "C:\\Users\\kyawtun\\Documents\\Visual Studio 2008\\Projects\\nckit\\internal\\c++\\compilers\\msvc800_prj\\dll\\bin\\DebugDLL";
			array<String^>^ dllFileNames = gcnew array<String^> {"ncbi_core.dll", "ncbi_general.dll", 
				"ncbi_pub.dll", "ncbi_seq.dll", "ncbi_misc.dll", 
				"ncbi_seqext.dll", 
				"ncbi_xreader.dll", "ncbi_xreader_id2.dll", "ncbi_xreader_id1.dll", 
				"ncbi_xreader_cache.dll", "ncbi_xloader_genbank.dll", 
				"ncbi_xobjsimple.dll", "ncbi_algo.dll"};

			for (int i = 0; i < dllFileNames->Length; i++)
			{
				String ^ fileName = dllFileNames[i];
				LoadLibrary(Path::Combine(basePath, fileName));
			}
		}

	};
}
