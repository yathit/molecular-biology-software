#include "muscle.h"
#include <stdio.h>
#include <new>

const int ONE_MB = 1024*1024;
const size_t RESERVE_BYTES = 8*ONE_MB;
static void *EmergencyReserve = 0;

void OnOutOfMemory()
	{
	free(EmergencyReserve);
	fprintf(stderr, "\n*** OUT OF MEMORY ***\n");
	fprintf(stderr, "Memory allocated so far %g MB\n", GetMemUseMB());
	SaveCurrentAlignment();
	exit(EXIT_FatalError);
	}

void SetNewHandler()
	{
	EmergencyReserve = malloc(RESERVE_BYTES);
	std::set_new_handler(OnOutOfMemory);
	}
