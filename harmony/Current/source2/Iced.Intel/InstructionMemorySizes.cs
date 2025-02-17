using System;

namespace Iced.Intel;

internal static class InstructionMemorySizes
{
	internal static ReadOnlySpan<byte> SizesNormal => new byte[4834]
	{
		0, 0, 0, 0, 0, 1, 2, 3, 5, 1,
		2, 3, 5, 0, 0, 0, 0, 0, 0, 0,
		0, 1, 2, 3, 5, 1, 2, 3, 5, 0,
		0, 0, 0, 0, 0, 0, 1, 2, 3, 5,
		1, 2, 3, 5, 0, 0, 0, 0, 0, 0,
		0, 0, 1, 2, 3, 5, 1, 2, 3, 5,
		0, 0, 0, 0, 0, 0, 0, 0, 1, 2,
		3, 5, 1, 2, 3, 5, 0, 0, 0, 0,
		0, 1, 2, 3, 5, 1, 2, 3, 5, 0,
		0, 0, 0, 0, 1, 2, 3, 5, 1, 2,
		3, 5, 0, 0, 0, 0, 0, 1, 2, 3,
		5, 1, 2, 3, 5, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 22, 23, 2, 2, 10, 11,
		11, 0, 0, 0, 10, 11, 12, 0, 0, 0,
		10, 11, 12, 1, 2, 3, 1, 2, 3, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 1, 1, 1,
		1, 1, 1, 1, 1, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 1,
		1, 1, 1, 1, 1, 1, 1, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 1, 2, 3, 5, 1, 2, 3, 5, 1,
		2, 3, 5, 1, 2, 3, 5, 2, 2, 2,
		0, 0, 0, 2, 2, 2, 2, 3, 5, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 1, 2, 3, 5, 1, 2, 3,
		5, 1, 2, 3, 5, 1, 2, 3, 5, 0,
		0, 0, 0, 1, 2, 3, 5, 1, 2, 3,
		5, 1, 2, 3, 5, 0, 0, 0, 0, 1,
		1, 1, 1, 1, 1, 1, 9, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 10, 11,
		12, 0, 0, 0, 0, 0, 0, 16, 17, 16,
		17, 1, 0, 2, 3, 5, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
		1, 1, 1, 9, 2, 3, 5, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 10, 11, 12, 1, 1,
		1, 1, 1, 1, 1, 9, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 10, 11, 12,
		0, 0, 0, 1, 29, 29, 29, 29, 29, 29,
		29, 29, 0, 0, 0, 0, 0, 0, 0, 0,
		29, 29, 29, 34, 35, 2, 34, 34, 35, 35,
		2, 2, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 11, 11, 11, 11, 11, 11, 11,
		11, 0, 0, 0, 0, 0, 11, 11, 11, 11,
		31, 31, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 30,
		30, 30, 30, 30, 30, 30, 30, 0, 0, 0,
		0, 0, 0, 0, 0, 30, 12, 30, 30, 36,
		37, 36, 36, 37, 37, 2, 2, 0, 0, 0,
		0, 0, 0, 10, 10, 10, 10, 10, 10, 10,
		10, 0, 0, 0, 0, 0, 0, 0, 0, 10,
		10, 10, 10, 42, 12, 42, 12, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 1, 1, 1, 9, 1, 9, 1, 9, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 10, 11,
		12, 2, 3, 5, 10, 11, 12, 2, 3, 5,
		10, 11, 12, 0, 0, 0, 0, 0, 0, 1,
		1, 2, 3, 5, 2, 3, 5, 19, 20, 21,
		16, 17, 18, 19, 20, 21, 16, 17, 18, 2,
		3, 5, 2, 2, 2, 2, 2, 2, 2, 2,
		2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
		19, 20, 26, 26, 27, 26, 26, 27, 26, 26,
		27, 26, 26, 27, 2, 2, 2, 5, 2, 2,
		2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 2, 2, 2, 2, 2, 2, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		2, 3, 5, 1, 1, 1, 0, 1, 2, 3,
		1, 2, 3, 74, 74, 90, 74, 90, 106, 75,
		75, 91, 75, 91, 107, 29, 0, 29, 0, 29,
		30, 0, 30, 0, 30, 74, 74, 90, 74, 90,
		106, 75, 75, 91, 75, 91, 107, 29, 0, 29,
		0, 29, 30, 0, 30, 0, 30, 0, 63, 0,
		63, 0, 63, 30, 30, 30, 74, 74, 90, 74,
		90, 106, 30, 30, 91, 30, 91, 107, 63, 63,
		63, 30, 30, 30, 74, 74, 90, 74, 90, 106,
		75, 75, 91, 75, 91, 107, 74, 74, 90, 74,
		90, 106, 75, 75, 91, 75, 91, 107, 0, 0,
		0, 63, 63, 63, 30, 30, 30, 74, 74, 90,
		74, 90, 106, 63, 63, 63, 30, 30, 30, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 2, 3, 5,
		2, 3, 5, 1, 1, 1, 1, 0, 24, 25,
		3, 5, 3, 5, 0, 24, 25, 3, 5, 3,
		5, 1, 0, 0, 0, 0, 2, 3, 5, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 74,
		74, 90, 74, 90, 106, 75, 75, 91, 75, 91,
		107, 74, 74, 90, 74, 90, 106, 75, 75, 91,
		75, 91, 107, 61, 61, 11, 12, 11, 12, 11,
		12, 11, 12, 11, 12, 11, 12, 74, 74, 90,
		74, 90, 106, 75, 75, 91, 75, 91, 107, 29,
		30, 63, 75, 29, 29, 29, 29, 29, 29, 30,
		30, 30, 30, 30, 30, 63, 75, 29, 29, 29,
		29, 29, 29, 30, 30, 30, 30, 30, 30, 29,
		29, 29, 30, 30, 30, 29, 30, 29, 30, 29,
		30, 0, 0, 0, 0, 0, 0, 0, 0, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		74, 74, 90, 74, 90, 106, 75, 75, 91, 75,
		91, 107, 29, 29, 29, 30, 30, 30, 74, 74,
		90, 29, 29, 74, 74, 90, 29, 29, 74, 74,
		90, 74, 90, 106, 75, 75, 91, 75, 91, 107,
		74, 74, 90, 74, 90, 106, 75, 75, 91, 75,
		91, 107, 74, 74, 90, 74, 90, 106, 75, 75,
		91, 75, 91, 107, 74, 74, 90, 74, 90, 106,
		75, 75, 91, 75, 91, 107, 74, 74, 90, 74,
		90, 106, 75, 75, 91, 75, 91, 107, 29, 29,
		29, 30, 30, 30, 74, 74, 90, 74, 90, 106,
		75, 75, 91, 75, 91, 107, 29, 29, 29, 30,
		30, 30, 63, 63, 74, 63, 74, 90, 75, 75,
		91, 75, 91, 107, 29, 29, 29, 30, 30, 30,
		69, 69, 83, 69, 83, 100, 72, 86, 103, 74,
		74, 90, 74, 90, 106, 74, 74, 90, 74, 90,
		106, 74, 74, 90, 74, 90, 106, 75, 75, 91,
		75, 91, 107, 29, 29, 29, 30, 30, 30, 74,
		74, 90, 74, 90, 106, 75, 75, 91, 75, 91,
		107, 29, 29, 29, 30, 30, 30, 74, 74, 90,
		74, 90, 106, 75, 75, 91, 75, 91, 107, 29,
		29, 29, 30, 30, 30, 74, 74, 90, 74, 90,
		106, 75, 75, 91, 75, 91, 107, 29, 29, 29,
		30, 30, 30, 50, 64, 64, 78, 64, 78, 95,
		52, 66, 66, 80, 66, 80, 97, 11, 68, 68,
		82, 68, 82, 99, 59, 67, 67, 81, 67, 81,
		98, 57, 65, 65, 79, 65, 79, 96, 59, 67,
		67, 81, 67, 81, 98, 61, 69, 69, 83, 69,
		83, 100, 59, 67, 67, 81, 67, 81, 98, 56,
		64, 64, 78, 64, 78, 95, 58, 66, 66, 80,
		66, 80, 97, 60, 68, 68, 82, 68, 82, 99,
		61, 69, 69, 83, 69, 83, 100, 71, 71, 85,
		71, 85, 102, 71, 71, 85, 71, 85, 102, 3,
		5, 3, 5, 3, 5, 3, 5, 5, 68, 68,
		82, 68, 82, 99, 71, 85, 102, 68, 68, 82,
		68, 82, 99, 71, 85, 102, 64, 78, 95, 66,
		80, 97, 58, 68, 68, 82, 68, 82, 99, 66,
		66, 80, 66, 80, 97, 66, 66, 80, 66, 80,
		97, 0, 0, 0, 0, 66, 80, 97, 0, 0,
		0, 0, 67, 81, 98, 0, 0, 0, 0, 66,
		80, 97, 68, 82, 99, 71, 85, 102, 68, 82,
		99, 71, 85, 102, 0, 0, 0, 0, 68, 82,
		99, 0, 0, 0, 0, 69, 83, 100, 72, 86,
		103, 0, 0, 0, 0, 68, 82, 99, 0, 0,
		0, 0, 71, 85, 102, 0, 0, 0, 6, 87,
		104, 0, 0, 0, 0, 71, 85, 102, 0, 0,
		0, 6, 87, 104, 56, 64, 64, 78, 64, 78,
		95, 58, 66, 66, 80, 66, 80, 97, 60, 68,
		68, 82, 68, 82, 99, 0, 0, 0, 3, 5,
		74, 90, 106, 75, 91, 107, 0, 63, 74, 90,
		75, 91, 107, 29, 29, 0, 30, 30, 3, 5,
		74, 90, 106, 75, 91, 107, 0, 63, 74, 90,
		75, 91, 107, 29, 29, 0, 30, 30, 63, 74,
		90, 75, 91, 107, 60, 68, 82, 71, 85, 102,
		68, 82, 99, 71, 85, 102, 63, 74, 90, 75,
		91, 107, 3, 5, 3, 5, 75, 75, 91, 74,
		74, 90, 75, 75, 91, 74, 74, 90, 3, 5,
		3, 5, 3, 5, 3, 5, 5, 5, 5, 5,
		68, 68, 82, 68, 82, 99, 71, 85, 102, 68,
		68, 82, 68, 82, 99, 71, 85, 102, 64, 78,
		95, 66, 80, 97, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 2, 5,
		1, 3, 2, 5, 1, 3, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 2, 3, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 2, 3, 1,
		2, 3, 0, 0, 0, 0, 0, 0, 0, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 38, 39,
		0, 0, 38, 39, 0, 0, 3, 0, 0, 3,
		3, 0, 0, 3, 40, 41, 3, 5, 40, 41,
		0, 0, 40, 41, 1, 0, 0, 5, 0, 0,
		0, 0, 0, 1, 1, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		10, 11, 12, 1, 2, 3, 5, 16, 17, 18,
		2, 3, 5, 16, 17, 18, 16, 17, 18, 1,
		1, 1, 2, 2, 2, 0, 0, 2, 3, 5,
		2, 3, 5, 2, 3, 5, 2, 3, 5, 2,
		3, 5, 2, 3, 5, 2, 3, 5, 2, 3,
		5, 2, 3, 5, 2, 3, 5, 2, 3, 5,
		9, 9, 9, 10, 10, 10, 1, 2, 3, 5,
		74, 74, 90, 74, 90, 106, 75, 75, 91, 75,
		91, 107, 29, 29, 29, 30, 30, 30, 3, 5,
		2, 2, 2, 2, 2, 2, 2, 2, 0, 0,
		0, 0, 0, 0, 0, 0, 74, 74, 90, 74,
		90, 106, 75, 75, 91, 75, 91, 107, 5, 6,
		40, 41, 40, 41, 40, 41, 5, 5, 5, 0,
		0, 0, 5, 0, 0, 0, 0, 0, 0, 0,
		0, 75, 75, 91, 74, 74, 90, 58, 66, 66,
		66, 66, 66, 66, 60, 68, 68, 68, 68, 68,
		68, 5, 71, 71, 71, 71, 71, 71, 5, 71,
		71, 85, 71, 85, 102, 59, 67, 67, 81, 67,
		81, 98, 5, 5, 5, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 56, 64, 64, 78, 64,
		78, 95, 58, 66, 66, 80, 66, 80, 97, 56,
		64, 64, 78, 64, 78, 95, 5, 6, 6, 87,
		68, 82, 99, 71, 85, 102, 56, 64, 64, 78,
		64, 78, 95, 58, 66, 66, 80, 66, 80, 97,
		56, 64, 64, 78, 64, 78, 95, 5, 6, 6,
		87, 68, 82, 99, 71, 85, 102, 56, 64, 64,
		78, 64, 78, 95, 59, 67, 67, 67, 67, 67,
		67, 61, 69, 69, 69, 69, 69, 69, 72, 72,
		72, 58, 66, 66, 80, 66, 80, 97, 58, 66,
		66, 80, 66, 80, 97, 59, 67, 67, 81, 67,
		81, 98, 75, 75, 91, 75, 91, 107, 61, 61,
		69, 61, 69, 83, 72, 86, 103, 75, 75, 91,
		75, 91, 107, 60, 68, 68, 82, 68, 82, 99,
		57, 65, 65, 79, 65, 79, 96, 59, 67, 67,
		81, 67, 81, 98, 59, 67, 67, 81, 67, 81,
		98, 5, 6, 6, 87, 68, 82, 99, 71, 85,
		102, 57, 65, 65, 79, 65, 79, 96, 59, 67,
		67, 81, 67, 81, 98, 59, 67, 67, 81, 67,
		81, 98, 5, 6, 6, 87, 68, 82, 99, 71,
		85, 102, 6, 6, 7, 58, 66, 66, 66, 66,
		66, 66, 60, 68, 68, 68, 68, 68, 68, 5,
		71, 71, 71, 71, 71, 71, 60, 68, 68, 82,
		68, 82, 99, 59, 67, 67, 81, 67, 81, 98,
		56, 64, 64, 78, 64, 78, 95, 5, 6, 6,
		56, 64, 64, 78, 64, 78, 95, 58, 66, 66,
		80, 66, 80, 97, 60, 68, 68, 82, 68, 82,
		99, 12, 71, 71, 85, 71, 85, 102, 56, 64,
		64, 78, 64, 78, 95, 58, 66, 66, 80, 66,
		80, 97, 60, 68, 68, 82, 68, 82, 99, 2,
		3, 5, 56, 64, 64, 78, 64, 78, 95, 58,
		66, 66, 80, 60, 68, 68, 82, 59, 67, 67,
		81, 57, 65, 65, 79, 65, 79, 96, 58, 66,
		66, 80, 60, 68, 68, 82, 59, 67, 67, 81,
		57, 65, 65, 79, 59, 67, 67, 81, 61, 69,
		69, 83, 59, 67, 67, 81, 67, 81, 98, 74,
		90, 74, 90, 106, 75, 91, 75, 91, 107, 74,
		90, 75, 91, 64, 66, 80, 97, 56, 64, 78,
		66, 80, 97, 50, 56, 64, 66, 80, 97, 48,
		50, 56, 62, 73, 62, 73, 89, 58, 66, 80,
		74, 68, 82, 99, 71, 85, 102, 52, 58, 66,
		75, 68, 82, 99, 71, 85, 102, 60, 68, 82,
		90, 90, 106, 91, 107, 6, 6, 7, 29, 29,
		29, 29, 29, 30, 63, 63, 30, 30, 32, 74,
		74, 75, 75, 90, 91, 57, 65, 65, 79, 65,
		79, 96, 59, 67, 67, 81, 67, 81, 98, 61,
		69, 69, 83, 69, 83, 100, 72, 86, 103, 57,
		57, 65, 57, 65, 79, 57, 65, 79, 51, 51,
		57, 51, 57, 65, 51, 57, 65, 49, 49, 51,
		49, 51, 57, 49, 51, 57, 59, 59, 67, 59,
		67, 81, 59, 67, 81, 53, 53, 59, 53, 59,
		67, 53, 59, 67, 61, 61, 69, 61, 69, 83,
		61, 69, 83, 64, 78, 95, 66, 80, 97, 64,
		78, 95, 66, 80, 97, 68, 82, 99, 71, 85,
		102, 68, 82, 99, 71, 85, 102, 69, 69, 83,
		69, 83, 100, 0, 0, 0, 0, 0, 0, 71,
		71, 85, 71, 85, 102, 0, 0, 0, 0, 0,
		0, 6, 6, 7, 6, 7, 8, 0, 0, 0,
		69, 69, 83, 69, 83, 100, 74, 90, 74, 90,
		106, 75, 91, 107, 75, 91, 29, 30, 74, 90,
		75, 91, 56, 56, 64, 56, 64, 78, 56, 64,
		78, 50, 50, 56, 50, 56, 64, 50, 56, 64,
		48, 48, 50, 48, 50, 56, 48, 50, 56, 58,
		58, 66, 58, 66, 80, 58, 66, 80, 52, 52,
		58, 52, 58, 66, 52, 58, 66, 60, 60, 68,
		60, 68, 82, 60, 68, 82, 82, 82, 99, 85,
		102, 72, 72, 86, 72, 86, 103, 65, 65, 79,
		65, 79, 96, 0, 0, 0, 0, 0, 0, 69,
		69, 83, 69, 83, 100, 72, 86, 103, 0, 0,
		0, 0, 0, 0, 66, 66, 80, 66, 80, 97,
		0, 0, 0, 68, 68, 82, 68, 82, 99, 71,
		85, 102, 65, 65, 79, 65, 79, 96, 69, 69,
		83, 69, 83, 100, 72, 86, 103, 66, 66, 80,
		66, 80, 97, 68, 68, 82, 68, 82, 99, 71,
		85, 102, 69, 69, 83, 69, 83, 100, 72, 86,
		103, 66, 66, 74, 90, 106, 75, 91, 107, 29,
		30, 68, 82, 99, 71, 85, 102, 68, 82, 71,
		85, 68, 82, 99, 71, 85, 102, 68, 82, 68,
		82, 99, 71, 85, 102, 68, 82, 71, 85, 68,
		82, 99, 71, 85, 102, 74, 90, 106, 75, 91,
		107, 29, 30, 74, 90, 106, 75, 91, 107, 29,
		30, 65, 79, 96, 65, 79, 96, 67, 81, 98,
		77, 94, 109, 67, 67, 81, 98, 67, 64, 78,
		95, 66, 80, 97, 68, 82, 99, 71, 85, 102,
		11, 11, 11, 11, 11, 12, 12, 60, 60, 60,
		12, 12, 12, 13, 68, 68, 71, 71, 82, 85,
		64, 78, 95, 66, 80, 97, 64, 78, 95, 66,
		80, 97, 68, 82, 99, 71, 85, 102, 74, 90,
		106, 75, 91, 107, 64, 78, 95, 66, 80, 97,
		68, 82, 99, 71, 85, 102, 66, 80, 97, 68,
		82, 99, 71, 85, 102, 66, 80, 97, 74, 90,
		106, 74, 90, 106, 68, 82, 99, 71, 85, 102,
		64, 78, 95, 66, 80, 97, 68, 82, 99, 71,
		85, 102, 74, 90, 106, 75, 91, 107, 9, 9,
		9, 9, 9, 10, 10, 10, 10, 10, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		64, 78, 95, 66, 80, 97, 68, 82, 99, 71,
		85, 102, 74, 90, 106, 75, 91, 107, 6, 6,
		6, 6, 6, 6, 71, 85, 102, 74, 90, 106,
		75, 91, 107, 68, 82, 99, 71, 85, 102, 74,
		90, 106, 75, 91, 107, 68, 82, 99, 71, 85,
		102, 68, 82, 71, 85, 64, 78, 95, 66, 80,
		97, 68, 82, 71, 85, 64, 78, 95, 11, 11,
		12, 12, 11, 11, 11, 12, 12, 12, 11, 11,
		12, 12, 11, 11, 11, 12, 12, 12, 29, 29,
		30, 30, 29, 29, 29, 30, 30, 30, 29, 29,
		30, 30, 29, 29, 29, 30, 30, 30, 74, 90,
		75, 91, 74, 90, 106, 75, 91, 107, 74, 90,
		75, 91, 74, 90, 106, 75, 91, 107, 74, 90,
		75, 91, 74, 90, 106, 75, 91, 107, 29, 30,
		29, 30, 74, 90, 75, 91, 74, 90, 106, 75,
		91, 107, 74, 29, 30, 29, 30, 74, 74, 90,
		75, 91, 74, 90, 106, 75, 91, 107, 29, 30,
		29, 30, 74, 90, 75, 91, 74, 90, 106, 75,
		91, 107, 29, 30, 29, 30, 11, 11, 11, 12,
		12, 12, 11, 11, 11, 12, 12, 12, 29, 29,
		29, 30, 30, 30, 29, 29, 29, 30, 30, 30,
		74, 90, 75, 91, 74, 90, 106, 75, 91, 107,
		74, 90, 75, 91, 74, 90, 106, 75, 91, 107,
		74, 90, 75, 91, 74, 90, 106, 75, 91, 107,
		29, 30, 29, 30, 74, 90, 75, 91, 74, 90,
		106, 75, 91, 107, 74, 29, 30, 29, 30, 74,
		74, 90, 75, 91, 74, 90, 106, 75, 91, 107,
		29, 30, 29, 30, 74, 90, 75, 91, 74, 90,
		106, 75, 91, 107, 29, 30, 29, 30, 70, 84,
		101, 70, 84, 101, 74, 90, 75, 91, 74, 90,
		106, 75, 91, 107, 74, 90, 75, 91, 74, 90,
		106, 75, 91, 107, 74, 90, 75, 91, 74, 90,
		106, 75, 91, 107, 29, 30, 29, 30, 74, 90,
		75, 91, 74, 90, 106, 75, 91, 107, 29, 30,
		29, 30, 74, 90, 75, 91, 74, 90, 106, 75,
		91, 107, 29, 30, 29, 30, 74, 90, 75, 91,
		74, 90, 106, 75, 91, 107, 29, 30, 29, 30,
		68, 82, 99, 71, 85, 102, 29, 30, 29, 30,
		29, 30, 29, 30, 29, 30, 29, 30, 29, 30,
		29, 30, 68, 106, 107, 68, 68, 106, 107, 68,
		29, 30, 68, 106, 107, 68, 29, 30, 64, 64,
		78, 64, 78, 95, 6, 6, 6, 6, 87, 6,
		87, 104, 6, 6, 87, 6, 87, 104, 6, 6,
		87, 6, 87, 104, 6, 6, 87, 6, 87, 104,
		2, 3, 5, 1, 1, 2, 3, 5, 2, 3,
		5, 3, 5, 3, 5, 3, 5, 3, 5, 3,
		5, 3, 5, 3, 5, 3, 5, 3, 5, 3,
		5, 3, 5, 3, 5, 3, 5, 3, 5, 11,
		12, 3, 5, 8, 8, 8, 8, 8, 8, 8,
		8, 8, 3, 5, 85, 85, 102, 91, 91, 107,
		68, 82, 68, 82, 99, 71, 85, 102, 74, 90,
		74, 90, 106, 75, 91, 75, 91, 107, 92, 74,
		74, 90, 74, 90, 106, 75, 75, 91, 75, 91,
		107, 29, 29, 29, 30, 30, 30, 74, 74, 90,
		75, 75, 91, 66, 66, 80, 56, 64, 64, 78,
		64, 78, 95, 1, 1, 1, 1, 1, 1, 2,
		2, 2, 2, 2, 2, 3, 5, 3, 5, 3,
		5, 29, 29, 29, 29, 29, 29, 32, 74, 74,
		75, 75, 32, 74, 74, 75, 75, 90, 91, 90,
		91, 62, 73, 62, 73, 89, 68, 82, 99, 71,
		85, 102, 69, 83, 100, 72, 86, 103, 1, 1,
		1, 1, 1, 1, 29, 29, 29, 3, 5, 3,
		5, 3, 5, 90, 106, 91, 107, 68, 82, 99,
		71, 85, 102, 74, 90, 106, 75, 91, 107, 29,
		30, 0, 0, 0, 0, 0, 0, 0, 0, 13,
		68, 68, 71, 71, 13, 68, 68, 71, 71, 82,
		85, 82, 85, 64, 78, 95, 66, 80, 97, 65,
		79, 96, 67, 81, 98, 74, 74, 90, 75, 75,
		64, 64, 78, 64, 78, 95, 82, 99, 85, 102,
		71, 71, 85, 71, 85, 102, 87, 74, 90, 74,
		90, 75, 91, 75, 91, 74, 90, 75, 91, 64,
		78, 74, 90, 106, 75, 91, 107, 29, 30, 74,
		90, 106, 75, 91, 107, 29, 30, 74, 90, 106,
		75, 91, 107, 29, 30, 74, 90, 74, 90, 75,
		91, 75, 91, 74, 90, 74, 90, 75, 91, 75,
		91, 64, 64, 64, 64, 64, 64, 64, 64, 64,
		64, 64, 64, 74, 90, 106, 75, 91, 107, 29,
		30, 74, 90, 74, 90, 75, 91, 75, 91, 29,
		29, 30, 30, 74, 90, 74, 90, 75, 91, 75,
		91, 29, 29, 30, 30, 66, 80, 97, 68, 82,
		99, 71, 85, 102, 66, 80, 97, 68, 82, 99,
		71, 85, 102, 74, 90, 74, 90, 75, 91, 75,
		91, 29, 29, 30, 30, 74, 90, 74, 90, 75,
		91, 75, 91, 29, 29, 30, 30, 68, 64, 64,
		78, 64, 78, 95, 64, 64, 78, 64, 78, 95,
		6, 6, 3, 5, 67, 67, 69, 69, 69, 67,
		67, 69, 69, 69, 6, 7, 6, 7, 64, 64,
		67, 67, 64, 66, 68, 71, 65, 67, 69, 72,
		64, 66, 68, 71, 3, 5, 3, 5, 3, 5,
		3, 5, 3, 5, 3, 5, 3, 5, 3, 5,
		3, 5, 0, 0, 0, 0, 74, 90, 75, 91,
		29, 30, 64, 64, 66, 66, 68, 68, 71, 71,
		64, 64, 66, 66, 68, 68, 71, 71, 65, 65,
		67, 67, 69, 69, 72, 72, 65, 65, 65, 67,
		67, 69, 64, 64, 64, 66, 66, 68, 65, 67,
		69, 3, 5, 3, 3, 3, 3, 59, 61, 63,
		63, 63, 63, 63, 63, 63, 63, 63, 63, 63,
		63, 63, 63, 63, 63, 63, 63, 63, 63, 63,
		59, 60, 56, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
		1, 0, 0, 0, 43, 0, 43, 0, 44, 44,
		44, 0, 0, 0, 0, 0, 0, 0, 3, 3,
		0, 0, 0, 45, 45, 45, 45, 45, 45, 0,
		0, 0, 0, 0, 0, 56, 59, 58, 56, 59,
		56, 59, 56, 57, 57, 59, 58, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 46, 46, 47, 47, 0, 46, 46,
		47, 47, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 65, 79, 65, 79, 67, 81,
		67, 81, 0, 0, 0, 0, 0, 0, 2, 2,
		2, 0, 0, 73, 89, 105, 28, 73, 89, 105,
		28, 28, 69, 83, 100, 75, 91, 107, 62, 73,
		89, 54, 62, 73, 62, 73, 89, 54, 62, 73,
		62, 73, 89, 54, 62, 73, 73, 89, 105, 73,
		89, 105, 74, 90, 106, 72, 86, 103, 30, 28,
		28, 28, 28, 28, 28, 11, 12, 29, 62, 73,
		89, 54, 62, 73, 62, 73, 89, 54, 62, 73,
		73, 89, 105, 73, 89, 105, 28, 28, 28, 28,
		68, 82, 99, 71, 85, 102, 3, 5, 66, 80,
		97, 67, 81, 98, 73, 89, 105, 28, 76, 93,
		108, 76, 93, 108, 54, 54, 76, 93, 108, 76,
		93, 108, 54, 54, 73, 89, 105, 73, 89, 105,
		73, 89, 105, 73, 89, 105, 73, 89, 105, 73,
		89, 105, 73, 89, 105, 73, 89, 105, 73, 89,
		105, 73, 89, 105, 73, 89, 105, 73, 89, 105,
		28, 28, 28, 28, 28, 28, 73, 89, 105, 73,
		89, 105, 73, 89, 105, 73, 89, 105, 73, 89,
		105, 73, 89, 105, 28, 28, 28, 28, 28, 28,
		73, 89, 105, 28, 73, 89, 105, 28, 73, 89,
		105, 28, 73, 89, 105, 28, 73, 89, 105, 28,
		28, 28, 0, 0, 2, 2, 2, 2, 73, 89,
		105, 28, 73, 89, 105, 28, 73, 89, 105, 28,
		73, 89, 105, 28, 73, 89, 105, 28, 73, 89,
		105, 28, 73, 89, 105, 28, 73, 89, 105, 28,
		28, 0, 0, 0, 0, 1, 1, 1, 1, 1,
		1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 1, 1, 0, 0, 0, 0,
		0, 0, 0, 0, 3, 5, 3, 5, 3, 5,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0
	};

	internal static ReadOnlySpan<byte> SizesBcst => new byte[4834]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 123, 138, 153,
		0, 0, 0, 124, 139, 154, 0, 0, 0, 123,
		138, 153, 0, 0, 0, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 123, 138, 153, 0, 0, 0, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 123, 138, 153, 0, 0, 0, 124, 139, 154,
		0, 0, 0, 123, 138, 153, 0, 0, 0, 124,
		139, 154, 0, 0, 0, 123, 138, 153, 0, 0,
		0, 124, 139, 154, 0, 0, 0, 123, 138, 153,
		0, 0, 0, 124, 139, 154, 0, 0, 0, 123,
		138, 153, 0, 0, 0, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 123, 138, 153,
		0, 0, 0, 124, 139, 154, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 114, 123, 138, 0, 0,
		0, 124, 139, 154, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 118, 133, 148, 121, 136, 151, 0,
		0, 0, 123, 138, 153, 0, 0, 0, 123, 138,
		153, 0, 0, 0, 123, 138, 153, 0, 0, 0,
		124, 139, 154, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 123, 138, 153, 0, 0, 0, 124, 139,
		154, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		123, 138, 153, 0, 0, 0, 124, 139, 154, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 123, 138,
		153, 0, 0, 0, 124, 139, 154, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 117, 132, 147, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 118,
		133, 148, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 117, 132, 147,
		0, 0, 0, 0, 118, 133, 148, 0, 0, 0,
		120, 135, 150, 0, 0, 0, 120, 135, 150, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 117, 132, 147, 120, 135, 150, 117, 132,
		147, 120, 135, 150, 0, 0, 0, 0, 117, 132,
		147, 0, 0, 0, 0, 118, 133, 148, 121, 136,
		151, 0, 0, 0, 0, 117, 132, 147, 0, 0,
		0, 0, 120, 135, 150, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 120, 135, 150, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 117, 132, 147, 0, 0, 0, 0, 0,
		123, 138, 153, 124, 139, 154, 0, 114, 123, 138,
		124, 139, 154, 0, 0, 0, 0, 0, 0, 0,
		123, 138, 153, 124, 139, 154, 0, 114, 123, 138,
		124, 139, 154, 0, 0, 0, 0, 0, 114, 123,
		138, 124, 139, 154, 111, 117, 132, 120, 135, 150,
		117, 132, 147, 120, 135, 150, 114, 123, 138, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 123, 138, 153, 0, 0, 0, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 123,
		138, 153, 0, 0, 0, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 120, 135, 150, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		117, 132, 147, 120, 135, 150, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 117, 132, 147, 120, 135, 150, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 124, 139, 154, 0, 0,
		0, 112, 118, 133, 121, 136, 151, 0, 0, 0,
		124, 139, 154, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 117, 132, 147, 120, 135,
		150, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		127, 142, 157, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 117, 132,
		147, 0, 0, 0, 0, 120, 135, 150, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 123, 138, 153, 0, 0, 124, 139, 154, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 117, 132, 147, 120, 135, 150, 0, 0, 0,
		0, 117, 132, 147, 120, 135, 150, 0, 0, 0,
		0, 138, 153, 139, 154, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 118, 133, 148, 121, 136, 151, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 117, 132, 147, 120, 135,
		150, 117, 132, 147, 120, 135, 150, 0, 0, 0,
		126, 141, 158, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 120, 135, 150, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 118, 133, 148, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 132, 147, 135,
		150, 0, 0, 0, 121, 136, 151, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 118, 133, 148, 121, 136, 151, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 118, 133, 148, 121, 136, 151, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 0, 0, 0, 118, 133, 148, 121, 136,
		151, 0, 0, 123, 138, 153, 124, 139, 154, 0,
		0, 117, 132, 147, 120, 135, 150, 0, 0, 0,
		0, 117, 132, 147, 120, 135, 150, 0, 0, 117,
		132, 147, 120, 135, 150, 0, 0, 0, 0, 117,
		132, 147, 120, 135, 150, 123, 138, 153, 124, 139,
		154, 0, 0, 123, 138, 153, 124, 139, 154, 0,
		0, 117, 132, 147, 117, 132, 147, 125, 140, 156,
		129, 144, 159, 0, 125, 140, 156, 0, 0, 0,
		0, 0, 0, 0, 117, 132, 147, 120, 135, 150,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 117, 132, 147, 120, 135, 150, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 0, 0,
		117, 132, 147, 120, 135, 150, 0, 0, 0, 117,
		132, 147, 120, 135, 150, 0, 0, 0, 123, 138,
		153, 123, 138, 153, 117, 132, 147, 120, 135, 150,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 120, 135, 150, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 123, 138, 153, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 123, 138, 153, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 123, 138, 153, 124, 139, 154,
		0, 0, 0, 0, 123, 138, 153, 124, 139, 154,
		0, 0, 0, 0, 123, 138, 153, 124, 139, 154,
		0, 0, 0, 0, 0, 0, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 123, 138, 153, 124, 139, 154,
		0, 0, 0, 0, 0, 0, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 119, 134,
		149, 119, 134, 149, 0, 0, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 123, 138,
		153, 124, 139, 154, 0, 0, 0, 0, 0, 0,
		0, 0, 123, 138, 153, 124, 139, 154, 0, 0,
		0, 0, 0, 0, 0, 0, 123, 138, 153, 124,
		139, 154, 0, 0, 0, 0, 0, 0, 0, 0,
		123, 138, 153, 124, 139, 154, 0, 0, 0, 0,
		117, 132, 147, 120, 135, 150, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 153, 154, 0, 0, 153, 154, 0,
		0, 0, 0, 153, 154, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 135, 150, 0, 139, 154,
		0, 0, 117, 132, 147, 120, 135, 150, 0, 0,
		123, 138, 153, 0, 0, 124, 139, 154, 0, 0,
		0, 0, 123, 138, 153, 0, 0, 0, 124, 139,
		154, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 117, 132, 147, 120,
		135, 150, 118, 133, 148, 121, 136, 151, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 138, 153, 139, 154, 117, 132, 147,
		120, 135, 150, 123, 138, 153, 124, 139, 154, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 132, 147, 135, 150,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 123, 138, 153, 124, 139, 154, 0, 0, 123,
		138, 153, 124, 139, 154, 0, 0, 123, 138, 153,
		124, 139, 154, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 123, 138, 153, 124, 139, 154, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 117, 132,
		147, 120, 135, 150, 0, 0, 0, 117, 132, 147,
		120, 135, 150, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 120, 135, 150, 0, 0, 0, 120, 135, 150,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 122, 137, 152, 0, 122, 137, 152,
		0, 0, 118, 133, 148, 124, 139, 154, 113, 122,
		137, 110, 113, 122, 113, 122, 137, 110, 113, 122,
		113, 122, 137, 110, 113, 122, 122, 137, 152, 122,
		137, 152, 123, 138, 153, 121, 136, 151, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 113, 122,
		137, 110, 113, 122, 113, 122, 137, 110, 113, 122,
		122, 137, 152, 122, 137, 152, 0, 0, 0, 0,
		117, 132, 147, 120, 135, 150, 0, 0, 116, 131,
		146, 115, 130, 145, 122, 137, 152, 0, 128, 143,
		155, 128, 143, 155, 0, 0, 128, 143, 155, 128,
		143, 155, 0, 0, 122, 137, 152, 122, 137, 152,
		122, 137, 152, 122, 137, 152, 122, 137, 152, 122,
		137, 152, 122, 137, 152, 122, 137, 152, 122, 137,
		152, 122, 137, 152, 122, 137, 152, 122, 137, 152,
		0, 0, 0, 0, 0, 0, 122, 137, 152, 122,
		137, 152, 122, 137, 152, 122, 137, 152, 122, 137,
		152, 122, 137, 152, 0, 0, 0, 0, 0, 0,
		122, 137, 152, 0, 122, 137, 152, 0, 122, 137,
		152, 0, 122, 137, 152, 0, 122, 137, 152, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 122, 137,
		152, 0, 122, 137, 152, 0, 122, 137, 152, 0,
		122, 137, 152, 0, 122, 137, 152, 0, 122, 137,
		152, 0, 122, 137, 152, 0, 122, 137, 152, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0
	};
}
