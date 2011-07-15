#include <vector>
#include <string>

using namespace std;
using namespace System;
using namespace System::Collections::Generic;


namespace Muscle
{
	
	public ref class Multialign
	{
	public:
		
		static bool DoMuscle(List<String ^>^ seqs, List<String ^>^ ids, 
			List<String ^>^ alns);

	private:
		Multialign()
			{
			};

		static void _doMuscle(vector<string> seqs, vector<string> ids, 
			List<String ^>^ alns);
	};
	
}