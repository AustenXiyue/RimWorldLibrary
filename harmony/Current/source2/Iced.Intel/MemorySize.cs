namespace Iced.Intel;

internal enum MemorySize
{
	Unknown,
	UInt8,
	UInt16,
	UInt32,
	UInt52,
	UInt64,
	UInt128,
	UInt256,
	UInt512,
	Int8,
	Int16,
	Int32,
	Int64,
	Int128,
	Int256,
	Int512,
	SegPtr16,
	SegPtr32,
	SegPtr64,
	WordOffset,
	DwordOffset,
	QwordOffset,
	Bound16_WordWord,
	Bound32_DwordDword,
	Bnd32,
	Bnd64,
	Fword6,
	Fword10,
	Float16,
	Float32,
	Float64,
	Float80,
	Float128,
	BFloat16,
	FpuEnv14,
	FpuEnv28,
	FpuState94,
	FpuState108,
	Fxsave_512Byte,
	Fxsave64_512Byte,
	Xsave,
	Xsave64,
	Bcd,
	Tilecfg,
	Tile,
	SegmentDescSelector,
	KLHandleAes128,
	KLHandleAes256,
	Packed16_UInt8,
	Packed16_Int8,
	Packed32_UInt8,
	Packed32_Int8,
	Packed32_UInt16,
	Packed32_Int16,
	Packed32_Float16,
	Packed32_BFloat16,
	Packed64_UInt8,
	Packed64_Int8,
	Packed64_UInt16,
	Packed64_Int16,
	Packed64_UInt32,
	Packed64_Int32,
	Packed64_Float16,
	Packed64_Float32,
	Packed128_UInt8,
	Packed128_Int8,
	Packed128_UInt16,
	Packed128_Int16,
	Packed128_UInt32,
	Packed128_Int32,
	Packed128_UInt52,
	Packed128_UInt64,
	Packed128_Int64,
	Packed128_Float16,
	Packed128_Float32,
	Packed128_Float64,
	Packed128_2xFloat16,
	Packed128_2xBFloat16,
	Packed256_UInt8,
	Packed256_Int8,
	Packed256_UInt16,
	Packed256_Int16,
	Packed256_UInt32,
	Packed256_Int32,
	Packed256_UInt52,
	Packed256_UInt64,
	Packed256_Int64,
	Packed256_UInt128,
	Packed256_Int128,
	Packed256_Float16,
	Packed256_Float32,
	Packed256_Float64,
	Packed256_Float128,
	Packed256_2xFloat16,
	Packed256_2xBFloat16,
	Packed512_UInt8,
	Packed512_Int8,
	Packed512_UInt16,
	Packed512_Int16,
	Packed512_UInt32,
	Packed512_Int32,
	Packed512_UInt52,
	Packed512_UInt64,
	Packed512_Int64,
	Packed512_UInt128,
	Packed512_Float16,
	Packed512_Float32,
	Packed512_Float64,
	Packed512_2xFloat16,
	Packed512_2xBFloat16,
	Broadcast32_Float16,
	Broadcast64_UInt32,
	Broadcast64_Int32,
	Broadcast64_Float16,
	Broadcast64_Float32,
	Broadcast128_Int16,
	Broadcast128_UInt16,
	Broadcast128_UInt32,
	Broadcast128_Int32,
	Broadcast128_UInt52,
	Broadcast128_UInt64,
	Broadcast128_Int64,
	Broadcast128_Float16,
	Broadcast128_Float32,
	Broadcast128_Float64,
	Broadcast128_2xInt16,
	Broadcast128_2xInt32,
	Broadcast128_2xUInt32,
	Broadcast128_2xFloat16,
	Broadcast128_2xBFloat16,
	Broadcast256_Int16,
	Broadcast256_UInt16,
	Broadcast256_UInt32,
	Broadcast256_Int32,
	Broadcast256_UInt52,
	Broadcast256_UInt64,
	Broadcast256_Int64,
	Broadcast256_Float16,
	Broadcast256_Float32,
	Broadcast256_Float64,
	Broadcast256_2xInt16,
	Broadcast256_2xInt32,
	Broadcast256_2xUInt32,
	Broadcast256_2xFloat16,
	Broadcast256_2xBFloat16,
	Broadcast512_Int16,
	Broadcast512_UInt16,
	Broadcast512_UInt32,
	Broadcast512_Int32,
	Broadcast512_UInt52,
	Broadcast512_UInt64,
	Broadcast512_Int64,
	Broadcast512_Float16,
	Broadcast512_Float32,
	Broadcast512_Float64,
	Broadcast512_2xFloat16,
	Broadcast512_2xInt16,
	Broadcast512_2xUInt32,
	Broadcast512_2xInt32,
	Broadcast512_2xBFloat16
}
