#include "muscle.h"
#include "distfunc.h"
#include "distcalc.h"
#include "msa.h"

void DistCalcDF::Init(const DistFunc &DF)
	{
	m_ptrDF = &DF;
	}

void DistCalcDF::CalcDistRange(unsigned i, dist_t Dist[]) const
	{
	for (unsigned j = 0; j < i; ++j)
		Dist[j] = m_ptrDF->GetDist(i, j);
	}

unsigned DistCalcDF::GetCount() const
	{
	return m_ptrDF->GetCount();
	}

unsigned DistCalcDF::GetId(unsigned i) const
	{
	return m_ptrDF->GetId(i);
	}

const char *DistCalcDF::GetName(unsigned i) const
	{
	return m_ptrDF->GetName(i);
	}

void DistCalcMSA::Init(const MSA &msa, DISTANCE Distance)
	{
	m_ptrMSA = &msa;
	m_Distance = Distance;
	}

void DistCalcMSA::CalcDistRange(unsigned i, dist_t Dist[]) const
	{
//	const unsigned uSeqIndex1 = m_ptrMSA->GetSeqIndex(i);
	for (unsigned j = 0; j < i; ++j)
		{
//		const unsigned uSeqIndex2 = m_ptrMSA->GetSeqIndex(j);
		const float PctId = (float) m_ptrMSA->GetPctIdentityPair(i, j);
		switch (m_Distance)
			{
		case DISTANCE_PctIdKimura:
			Dist[j] = (float) KimuraDist(PctId);
			break;
		case DISTANCE_PctIdLog:
			Dist[j] = (float) PctIdToMAFFTDist(PctId);
			break;
		default:
			Quit("DistCalcMSA: Invalid DISTANCE_%u", m_Distance);
			}
		}
	}

unsigned DistCalcMSA::GetCount() const
	{
	return m_ptrMSA->GetSeqCount();
	}

unsigned DistCalcMSA::GetId(unsigned i) const
	{
	return m_ptrMSA->GetSeqId(i);
	}

const char *DistCalcMSA::GetName(unsigned i) const
	{
	return m_ptrMSA->GetSeqName(i);
	}
