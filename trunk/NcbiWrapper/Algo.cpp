// This is the main DLL file.

#include "stdafx.h"

#include "Algo.h"


using namespace System::Collections::Generic;
using namespace System::Text;
using namespace System::Runtime::InteropServices;
using namespace System::Windows;
using namespace System::Windows::Controls;
using namespace System::IO;
using namespace BaseLibrary;
using namespace System::ComponentModel;
using namespace NcbiWrapper;


[DllImport("ncbi_algo.dll", CharSet = CharSet::Ansi)]
extern "C" int xadd(int a, int b);

[DllImport("ncbi_algo.dll", CharSet = CharSet::Ansi)]
extern "C" bool nwalign([MarshalAs(UnmanagedType::LPStr)] String ^ idA, [MarshalAs(UnmanagedType::LPStr)] String ^ seqA, int lenSeqA,
					   [MarshalAs(UnmanagedType::LPStr)] String ^ idB, [MarshalAs(UnmanagedType::LPStr)] String ^ seqB, int lenSeqB, 
					   [In][Out] int * score, 
					   [In][Out] char * result, int lenResult);


NcbiWrapper::AlignmentResult ^ NcbiWrapper::Algo::Nwalign(String ^ idA, String ^ seqA, 
														String ^ idB, String ^ seqB)
{
	try
	{
		// StringBuilder ^ result = gcnew StringBuilder();
		int score; // alignment score

		int headerSize = idA->Length + idB->Length + 30;
		int numberOfLines = 3;
		int maximunPossibleLength = seqA->Length + seqB->Length;
		int resultSize = (maximunPossibleLength) * numberOfLines + headerSize;
		char* result = new char[resultSize];

		bool ok = nwalign(idA, seqA, seqA->Length, idB, seqB, seqB->Length, &score, result, resultSize);
		AlignmentResult ^ ans = gcnew AlignmentResult;
		ans->Alignment = gcnew String(result);
		ans->Score = score;

		//FIXME: Wanna to delete result pointer
		return ans;
	}
	catch (Exception ^ dllErr)
	{
		Int32 err = Marshal::GetLastWin32Error();
		throw gcnew Win32Exception("\n" + dllErr->Message + " (" + err + ")");
	}
} 