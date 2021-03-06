#include "muscle.h"

// Note: We use 32x32 arrays rather than 20x20 as this may give the compiler
// optimizer an opportunity to make subscript arithmetic more efficient
// (multiplying by 32 is same as shifting left by 5 bits).

#define v(x)	((float) x)
#define ROW(A, C, D, E, F, G, H, I, K, L, M, N, P, Q, R, S, T, V, W, Y) \
	{ v(A), v(C), v(D), v(E), v(F), v(G), v(H), v(I), v(K), v(L), v(M), v(N), v(P), v(Q), \
	v(R), v(S), v(T), v(V), v(W), v(Y) },


//         A        C        D        E        F        G        H        I        K        L
//         M        N        P        Q        R        S        T        V        W        Y
// VTML200
float VTML_LA[32][32] =
	{
ROW( 2.25080, 1.31180, 0.82704, 0.88740, 0.55520, 1.09860, 0.71673, 0.80805, 0.81213, 0.68712,
     0.79105, 0.86777, 0.99328, 0.86644, 0.72821, 1.33924, 1.20373, 1.05956, 0.38107, 0.54373) // A

ROW( 1.31180,15.79469, 0.39862, 0.42329, 0.49882, 0.65541, 0.67100, 0.97185, 0.46414, 0.55673,
     0.90230, 0.63236, 0.54479, 0.47895, 0.56465, 1.18490, 0.99069, 1.21604, 0.28988, 0.91338) // C

ROW( 0.82704, 0.39862, 4.18833, 2.06850, 0.25194, 0.90937, 1.01617, 0.32860, 1.03391, 0.31300,
     0.42498, 1.80888, 0.81307, 1.20043, 0.63712, 1.03001, 0.88191, 0.43557, 0.26313, 0.37947) // D

ROW( 0.88740, 0.42329, 2.06850, 3.08354, 0.33456, 0.77183, 0.94536, 0.43151, 1.35989, 0.45579,
     0.53423, 1.15745, 0.82832, 1.66752, 0.84500, 0.98693, 0.88132, 0.54047, 0.24519, 0.52025) // E

ROW( 0.55520, 0.49882, 0.25194, 0.33456, 6.08351, 0.30140, 1.02191, 1.10969, 0.37069, 1.50587,
     1.41207, 0.42850, 0.41706, 0.48113, 0.41970, 0.56867, 0.57172, 0.91256, 2.02494, 3.44675) // F

ROW( 1.09860, 0.65541, 0.90937, 0.77183, 0.30140, 5.62829, 0.64191, 0.28432, 0.67874, 0.30549,
     0.37739, 1.01012, 0.60851, 0.65996, 0.63660, 1.03448, 0.68435, 0.40728, 0.36034, 0.35679) // G

ROW( 0.71673, 0.67100, 1.01617, 0.94536, 1.02191, 0.64191, 6.05494, 0.50783, 1.03822, 0.60887,
     0.55685, 1.28619, 0.72275, 1.41503, 1.24635, 0.93344, 0.83543, 0.54817, 0.81780, 1.81552) // H

ROW( 0.80805, 0.97185, 0.32860, 0.43151, 1.10969, 0.28432, 0.50783, 3.03766, 0.49310, 1.88886,
     1.75039, 0.44246, 0.44431, 0.53213, 0.48153, 0.55603, 0.88168, 2.37367, 0.68494, 0.70035) // I

ROW( 0.81213, 0.46414, 1.03391, 1.35989, 0.37069, 0.67874, 1.03822, 0.49310, 2.72883, 0.52739,
     0.68244, 1.15671, 0.82911, 1.51333, 2.33521, 0.93858, 0.92730, 0.55467, 0.39944, 0.52549) // K

ROW( 0.68712, 0.55673, 0.31300, 0.45579, 1.50587, 0.30549, 0.60887, 1.88886, 0.52739, 3.08540,
     2.14480, 0.43539, 0.53630, 0.62771, 0.53025, 0.53468, 0.69924, 1.50372, 0.82822, 0.89854) // L

ROW( 0.79105, 0.90230, 0.42498, 0.53423, 1.41207, 0.37739, 0.55685, 1.75039, 0.68244, 2.14480,
     4.04057, 0.55603, 0.48415, 0.76770, 0.66775, 0.62409, 0.87759, 1.42742, 0.52278, 0.72067) // M

ROW( 0.86777, 0.63236, 1.80888, 1.15745, 0.42850, 1.01012, 1.28619, 0.44246, 1.15671, 0.43539,
     0.55603, 3.36000, 0.69602, 1.13490, 0.98603, 1.31366, 1.11252, 0.50603, 0.35810, 0.68349) // N

ROW( 0.99328, 0.54479, 0.81307, 0.82832, 0.41706, 0.60851, 0.72275, 0.44431, 0.82911, 0.53630,
     0.48415, 0.69602, 7.24709, 0.90276, 0.74827, 1.03719, 0.83014, 0.56795, 0.37867, 0.33127) // P

ROW( 0.86644, 0.47895, 1.20043, 1.66752, 0.48113, 0.65996, 1.41503, 0.53213, 1.51333, 0.62771,
     0.76770, 1.13490, 0.90276, 2.86937, 1.50116, 0.99561, 0.93103, 0.61085, 0.29926, 0.51971) // Q

ROW( 0.72821, 0.56465, 0.63712, 0.84500, 0.41970, 0.63660, 1.24635, 0.48153, 2.33521, 0.53025,
     0.66775, 0.98603, 0.74827, 1.50116, 4.28698, 0.84662, 0.80673, 0.51422, 0.47569, 0.59592) // R

ROW( 1.33924, 1.18490, 1.03001, 0.98693, 0.56867, 1.03448, 0.93344, 0.55603, 0.93858, 0.53468,
     0.62409, 1.31366, 1.03719, 0.99561, 0.84662, 2.13816, 1.52911, 0.67767, 0.45129, 0.66767) // S

ROW( 1.20373, 0.99069, 0.88191, 0.88132, 0.57172, 0.68435, 0.83543, 0.88168, 0.92730, 0.69924,
     0.87759, 1.11252, 0.83014, 0.93103, 0.80673, 1.52911, 2.58221, 0.98702, 0.31541, 0.57954) // T

ROW( 1.05956, 1.21604, 0.43557, 0.54047, 0.91256, 0.40728, 0.54817, 2.37367, 0.55467, 1.50372,
     1.42742, 0.50603, 0.56795, 0.61085, 0.51422, 0.67767, 0.98702, 2.65580, 0.43419, 0.63805) // V

ROW( 0.38107, 0.28988, 0.26313, 0.24519, 2.02494, 0.36034, 0.81780, 0.68494, 0.39944, 0.82822,
     0.52278, 0.35810, 0.37867, 0.29926, 0.47569, 0.45129, 0.31541, 0.43419,31.39564, 2.51433) // W

ROW( 0.54373, 0.91338, 0.37947, 0.52025, 3.44675, 0.35679, 1.81552, 0.70035, 0.52549, 0.89854,
     0.72067, 0.68349, 0.33127, 0.51971, 0.59592, 0.66767, 0.57954, 0.63805, 2.51433, 7.50693) // Y
	};

const float VTML_SP_CENTER = (float) 22.0;

#undef	ROW
#undef	v
#define v(x)	((float) (x + VTML_SP_CENTER))
#define ROW(A, C, D, E, F, G, H, I, K, L, M, N, P, Q, R, S, T, V, W, Y, X) \
	{ v(A), v(C), v(D), v(E), v(F), v(G), v(H), v(I), v(K), v(L), v(M), v(N), v(P), v(Q), \
	v(R), v(S), v(T), v(V), v(W), v(Y), v(X) },

// VTML 240
float VTML_SP[32][32] =
	{
//     A    C    D    E    F    G    H    I    K    L    M    N    P    Q    R    S    T    V    W    Y  X
ROW(  58,  23, -12,  -7, -44,  10, -23, -14, -14, -27, -17,  -8,   1,  -9, -22,  23,  15,   5, -74, -45, 0) // A
ROW(  23, 224, -67, -63, -50, -30, -29,   1, -56, -41,  -6, -33, -44, -53, -43,  15,   2,  18, -93,  -6, 0) // C
ROW( -12, -67, 111,  59,-104,  -4,   4, -84,   6, -88, -65,  48, -13,  18, -29,   5,  -7, -63,-105, -73, 0) // D
ROW(  -7, -63,  59,  85, -83, -17,  -1, -63,  25, -60, -47,  15, -12,  40,  -8,   1,  -7, -47,-108, -51, 0) // E
ROW( -44, -50,-104, -83, 144, -93,   4,  12, -74,  36,  30, -64, -67, -56, -65, -43, -41,  -3,  63, 104, 0) // F
ROW(  10, -30,  -4, -17, -93, 140, -32, -95, -27, -91, -75,   4, -36, -29, -32,   5, -26, -68, -80, -79, 0) // G
ROW( -23, -29,   4,  -1,   4, -32, 137, -50,   6, -37, -42,  21, -23,  27,  19,  -4, -12, -44, -13,  48, 0) // H
ROW( -14,   1, -84, -63,  12, -95, -50,  86, -53,  53,  47, -62, -60, -47, -55, -43,  -8,  69, -27, -24, 0) // I
ROW( -14, -56,   6,  25, -74, -27,   6, -53,  75, -48, -30,  13, -12,  34,  68,  -3,  -4, -44, -71, -49, 0) // K
ROW( -27, -41, -88, -60,  36, -91, -37,  53, -48,  88,  62, -63, -48, -36, -48, -47, -25,  36, -11,  -4, 0) // L
ROW( -17,  -6, -65, -47,  30, -75, -42,  47, -30,  62, 103, -45, -54, -21, -31, -35,  -9,  31, -46, -20, 0) // M
ROW(  -8, -33,  48,  15, -64,   4,  21, -62,  13, -63, -45,  89, -25,  12,   2,  22,  10, -51, -79, -29, 0) // N
ROW(   1, -44, -13, -12, -67, -36, -23, -60, -12, -48, -54, -25, 160,  -6, -20,   5, -12, -42, -76, -83, 0) // P
ROW(  -9, -53,  18,  40, -56, -29,  27, -47,  34, -36, -21,  12,  -6,  75,  34,   1,  -4, -37, -92, -48, 0) // Q
ROW( -22, -43, -29,  -8, -65, -32,  19, -55,  68, -48, -31,   2, -20,  34, 113, -10, -14, -49, -58, -39, 0) // R
ROW(  23,  15,   5,   1, -43,   5,  -4, -43,  -3, -47, -35,  22,   5,   1, -10,  53,  32, -28, -62, -31, 0) // S
ROW(  15,   2,  -7,  -7, -41, -26, -12,  -8,  -4, -25,  -9,  10, -12,  -4, -14,  32,  68,   0, -87, -40, 0) // T
ROW(   5,  18, -63, -47,  -3, -68, -44,  69, -44,  36,  31, -51, -42, -37, -49, -28,   0,  74, -61, -32, 0) // V
ROW( -74, -93,-105,-108,  63, -80, -13, -27, -71, -11, -46, -79, -76, -92, -58, -62, -87, -61, 289,  81, 0) // W
ROW( -45,  -6, -73, -51, 104, -79,  48, -24, -49,  -4, -20, -29, -83, -48, -39, -31, -40, -32,  81, 162, 0) // Y
ROW(   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0) // X
	};

#undef	v
#define v(x)	((float) (x))
#define RNC(A, C, D, E, F, G, H, I, K, L, M, N, P, Q, R, S, T, V, W, Y, X) \
	{ v(A), v(C), v(D), v(E), v(F), v(G), v(H), v(I), v(K), v(L), v(M), v(N), v(P), v(Q), \
	v(R), v(S), v(T), v(V), v(W), v(Y), v(X) },

float VTML_SPNoCenter[32][32] =
	{
//     A    C    D    E    F    G    H    I    K    L    M    N    P    Q    R    S    T    V    W    Y  X
RNC(  58,  23, -12,  -7, -44,  10, -23, -14, -14, -27, -17,  -8,   1,  -9, -22,  23,  15,   5, -74, -45, 0) // A
RNC(  23, 224, -67, -63, -50, -30, -29,   1, -56, -41,  -6, -33, -44, -53, -43,  15,   2,  18, -93,  -6, 0) // C
RNC( -12, -67, 111,  59,-104,  -4,   4, -84,   6, -88, -65,  48, -13,  18, -29,   5,  -7, -63,-105, -73, 0) // D
RNC(  -7, -63,  59,  85, -83, -17,  -1, -63,  25, -60, -47,  15, -12,  40,  -8,   1,  -7, -47,-108, -51, 0) // E
RNC( -44, -50,-104, -83, 144, -93,   4,  12, -74,  36,  30, -64, -67, -56, -65, -43, -41,  -3,  63, 104, 0) // F
RNC(  10, -30,  -4, -17, -93, 140, -32, -95, -27, -91, -75,   4, -36, -29, -32,   5, -26, -68, -80, -79, 0) // G
RNC( -23, -29,   4,  -1,   4, -32, 137, -50,   6, -37, -42,  21, -23,  27,  19,  -4, -12, -44, -13,  48, 0) // H
RNC( -14,   1, -84, -63,  12, -95, -50,  86, -53,  53,  47, -62, -60, -47, -55, -43,  -8,  69, -27, -24, 0) // I
RNC( -14, -56,   6,  25, -74, -27,   6, -53,  75, -48, -30,  13, -12,  34,  68,  -3,  -4, -44, -71, -49, 0) // K
RNC( -27, -41, -88, -60,  36, -91, -37,  53, -48,  88,  62, -63, -48, -36, -48, -47, -25,  36, -11,  -4, 0) // L
RNC( -17,  -6, -65, -47,  30, -75, -42,  47, -30,  62, 103, -45, -54, -21, -31, -35,  -9,  31, -46, -20, 0) // M
RNC(  -8, -33,  48,  15, -64,   4,  21, -62,  13, -63, -45,  89, -25,  12,   2,  22,  10, -51, -79, -29, 0) // N
RNC(   1, -44, -13, -12, -67, -36, -23, -60, -12, -48, -54, -25, 160,  -6, -20,   5, -12, -42, -76, -83, 0) // P
RNC(  -9, -53,  18,  40, -56, -29,  27, -47,  34, -36, -21,  12,  -6,  75,  34,   1,  -4, -37, -92, -48, 0) // Q
RNC( -22, -43, -29,  -8, -65, -32,  19, -55,  68, -48, -31,   2, -20,  34, 113, -10, -14, -49, -58, -39, 0) // R
RNC(  23,  15,   5,   1, -43,   5,  -4, -43,  -3, -47, -35,  22,   5,   1, -10,  53,  32, -28, -62, -31, 0) // S
RNC(  15,   2,  -7,  -7, -41, -26, -12,  -8,  -4, -25,  -9,  10, -12,  -4, -14,  32,  68,   0, -87, -40, 0) // T
RNC(   5,  18, -63, -47,  -3, -68, -44,  69, -44,  36,  31, -51, -42, -37, -49, -28,   0,  74, -61, -32, 0) // V
RNC( -74, -93,-105,-108,  63, -80, -13, -27, -71, -11, -46, -79, -76, -92, -58, -62, -87, -61, 289,  81, 0) // W
RNC( -45,  -6, -73, -51, 104, -79,  48, -24, -49,  -4, -20, -29, -83, -48, -39, -31, -40, -32,  81, 162, 0) // Y
RNC(   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0) // X
	};
