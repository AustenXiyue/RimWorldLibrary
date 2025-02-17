using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[UsedByNativeCode]
[NativeType("Runtime/Graphics/CommandBuffer/RenderingCommandBuffer.h")]
[NativeHeader("Runtime/Export/Graphics/RenderingCommandBuffer.bindings.h")]
[NativeHeader("Runtime/Shaders/RayTracingShader.h")]
[NativeHeader("Runtime/Shaders/ComputeShader.h")]
public class CommandBuffer : IDisposable
{
	internal IntPtr m_Ptr;

	public extern string name
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int sizeInBytes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("GetBufferSize")]
		get;
	}

	public void ConvertTexture(RenderTargetIdentifier src, RenderTargetIdentifier dst)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		ConvertTexture_Internal(src, 0, dst, 0);
	}

	public void ConvertTexture(RenderTargetIdentifier src, int srcElement, RenderTargetIdentifier dst, int dstElement)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		ConvertTexture_Internal(src, srcElement, dst, dstElement);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddWaitAllAsyncReadbackRequests")]
	public extern void WaitAllAsyncReadbackRequests();

	public unsafe void RequestAsyncReadback(ComputeBuffer src, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_1(src, callback, null);
	}

	public unsafe void RequestAsyncReadback(ComputeBuffer src, int size, int offset, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_2(src, size, offset, callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_3(src, callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_4(src, mipIndex, callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, TextureFormat dstFormat, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_5(src, mipIndex, GraphicsFormatUtility.GetGraphicsFormat(dstFormat, QualitySettings.activeColorSpace == ColorSpace.Linear), callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, GraphicsFormat dstFormat, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_5(src, mipIndex, dstFormat, callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_6(src, mipIndex, x, width, y, height, z, depth, callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, TextureFormat dstFormat, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_7(src, mipIndex, x, width, y, height, z, depth, GraphicsFormatUtility.GetGraphicsFormat(dstFormat, QualitySettings.activeColorSpace == ColorSpace.Linear), callback, null);
	}

	public unsafe void RequestAsyncReadback(Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, GraphicsFormat dstFormat, Action<AsyncGPUReadbackRequest> callback)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_RequestAsyncReadback_7(src, mipIndex, x, width, y, height, z, depth, dstFormat, callback, null);
	}

	private unsafe static AsyncRequestNativeArrayData CreateAsyncRequestNativeArrayData<T>(ref NativeArray<T> output) where T : struct
	{
		AsyncRequestNativeArrayData result = default(AsyncRequestNativeArrayData);
		result.nativeArrayBuffer = output.GetUnsafePtr();
		result.lengthInBytes = output.Length * UnsafeUtility.SizeOf<T>();
		return result;
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, ComputeBuffer src, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_1(src, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, ComputeBuffer src, int size, int offset, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_2(src, size, offset, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_3(src, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_4(src, mipIndex, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, TextureFormat dstFormat, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_5(src, mipIndex, GraphicsFormatUtility.GetGraphicsFormat(dstFormat, QualitySettings.activeColorSpace == ColorSpace.Linear), callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, GraphicsFormat dstFormat, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_5(src, mipIndex, dstFormat, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_6(src, mipIndex, x, width, y, height, z, depth, callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, TextureFormat dstFormat, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_7(src, mipIndex, x, width, y, height, z, depth, GraphicsFormatUtility.GetGraphicsFormat(dstFormat, QualitySettings.activeColorSpace == ColorSpace.Linear), callback, &asyncRequestNativeArrayData);
	}

	public unsafe void RequestAsyncReadbackIntoNativeArray<T>(ref NativeArray<T> output, Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, GraphicsFormat dstFormat, Action<AsyncGPUReadbackRequest> callback) where T : struct
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		AsyncRequestNativeArrayData asyncRequestNativeArrayData = CreateAsyncRequestNativeArrayData(ref output);
		Internal_RequestAsyncReadback_7(src, mipIndex, x, width, y, height, z, depth, dstFormat, callback, &asyncRequestNativeArrayData);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_1([NotNull] ComputeBuffer src, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_2([NotNull] ComputeBuffer src, int size, int offset, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_3([NotNull] Texture src, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_4([NotNull] Texture src, int mipIndex, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_5([NotNull] Texture src, int mipIndex, GraphicsFormat dstFormat, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_6([NotNull] Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddRequestAsyncReadback")]
	private unsafe extern void Internal_RequestAsyncReadback_7([NotNull] Texture src, int mipIndex, int x, int width, int y, int height, int z, int depth, GraphicsFormat dstFormat, [NotNull] Action<AsyncGPUReadbackRequest> callback, AsyncRequestNativeArrayData* nativeArrayData = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddSetInvertCulling")]
	public extern void SetInvertCulling(bool invertCulling);

	private void ConvertTexture_Internal(RenderTargetIdentifier src, int srcElement, RenderTargetIdentifier dst, int dstElement)
	{
		ConvertTexture_Internal_Injected(ref src, srcElement, ref dst, dstElement);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetSinglePassStereo", HasExplicitThis = true)]
	private extern void Internal_SetSinglePassStereo(SinglePassStereoMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::InitBuffer")]
	private static extern IntPtr InitBuffer();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::CreateGPUFence_Internal", HasExplicitThis = true)]
	private extern IntPtr CreateGPUFence_Internal(GraphicsFenceType fenceType, SynchronisationStageFlags stage);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::WaitOnGPUFence_Internal", HasExplicitThis = true)]
	private extern void WaitOnGPUFence_Internal(IntPtr fencePtr, SynchronisationStageFlags stage);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::ReleaseBuffer", HasExplicitThis = true, IsThreadSafe = true)]
	private extern void ReleaseBuffer();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeFloatParam", HasExplicitThis = true)]
	public extern void SetComputeFloatParam([NotNull] ComputeShader computeShader, int nameID, float val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeIntParam", HasExplicitThis = true)]
	public extern void SetComputeIntParam([NotNull] ComputeShader computeShader, int nameID, int val);

	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeVectorParam", HasExplicitThis = true)]
	public void SetComputeVectorParam([NotNull] ComputeShader computeShader, int nameID, Vector4 val)
	{
		SetComputeVectorParam_Injected(computeShader, nameID, ref val);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeVectorArrayParam", HasExplicitThis = true)]
	public extern void SetComputeVectorArrayParam([NotNull] ComputeShader computeShader, int nameID, Vector4[] values);

	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeMatrixParam", HasExplicitThis = true)]
	public void SetComputeMatrixParam([NotNull] ComputeShader computeShader, int nameID, Matrix4x4 val)
	{
		SetComputeMatrixParam_Injected(computeShader, nameID, ref val);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeMatrixArrayParam", HasExplicitThis = true)]
	public extern void SetComputeMatrixArrayParam([NotNull] ComputeShader computeShader, int nameID, Matrix4x4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetComputeFloats", HasExplicitThis = true)]
	private extern void Internal_SetComputeFloats([NotNull] ComputeShader computeShader, int nameID, float[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetComputeInts", HasExplicitThis = true)]
	private extern void Internal_SetComputeInts([NotNull] ComputeShader computeShader, int nameID, int[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetComputeTextureParam", HasExplicitThis = true)]
	private extern void Internal_SetComputeTextureParam([NotNull] ComputeShader computeShader, int kernelIndex, int nameID, ref RenderTargetIdentifier rt, int mipLevel, RenderTextureSubElement element);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetComputeBufferParam", HasExplicitThis = true)]
	public extern void SetComputeBufferParam([NotNull] ComputeShader computeShader, int kernelIndex, int nameID, ComputeBuffer buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DispatchCompute", HasExplicitThis = true, ThrowsException = true)]
	private extern void Internal_DispatchCompute([NotNull] ComputeShader computeShader, int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DispatchComputeIndirect", HasExplicitThis = true, ThrowsException = true)]
	private extern void Internal_DispatchComputeIndirect([NotNull] ComputeShader computeShader, int kernelIndex, ComputeBuffer indirectBuffer, uint argsOffset);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingBufferParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingBufferParam([NotNull] RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingTextureParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingTextureParam([NotNull] RayTracingShader rayTracingShader, int nameID, ref RenderTargetIdentifier rt);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingFloatParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingFloatParam([NotNull] RayTracingShader rayTracingShader, int nameID, float val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingIntParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingIntParam([NotNull] RayTracingShader rayTracingShader, int nameID, int val);

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingVectorParam", HasExplicitThis = true)]
	private void Internal_SetRayTracingVectorParam([NotNull] RayTracingShader rayTracingShader, int nameID, Vector4 val)
	{
		Internal_SetRayTracingVectorParam_Injected(rayTracingShader, nameID, ref val);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingVectorArrayParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingVectorArrayParam([NotNull] RayTracingShader rayTracingShader, int nameID, Vector4[] values);

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingMatrixParam", HasExplicitThis = true)]
	private void Internal_SetRayTracingMatrixParam([NotNull] RayTracingShader rayTracingShader, int nameID, Matrix4x4 val)
	{
		Internal_SetRayTracingMatrixParam_Injected(rayTracingShader, nameID, ref val);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingMatrixArrayParam", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingMatrixArrayParam([NotNull] RayTracingShader rayTracingShader, int nameID, Matrix4x4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingFloats", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingFloats([NotNull] RayTracingShader rayTracingShader, int nameID, float[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingInts", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingInts([NotNull] RayTracingShader rayTracingShader, int nameID, int[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_BuildRayTracingAccelerationStructure", HasExplicitThis = true)]
	private extern void Internal_BuildRayTracingAccelerationStructure([NotNull] RayTracingAccelerationStructure accelerationStructure);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_SetRayTracingAccelerationStructure", HasExplicitThis = true)]
	private extern void Internal_SetRayTracingAccelerationStructure([NotNull] RayTracingShader rayTracingShader, int nameID, RayTracingAccelerationStructure accelerationStructure);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddSetRayTracingShaderPass")]
	public extern void SetRayTracingShaderPass([NotNull] RayTracingShader rayTracingShader, string passName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DispatchRays", HasExplicitThis = true, ThrowsException = true)]
	private extern void Internal_DispatchRays([NotNull] RayTracingShader rayTracingShader, string rayGenShaderName, uint width, uint height, uint depth, Camera camera = null);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddGenerateMips")]
	private extern void Internal_GenerateMips(RenderTexture rt);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddResolveAntiAliasedSurface")]
	private extern void Internal_ResolveAntiAliasedSurface(RenderTexture rt, RenderTexture target);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddCopyCounterValue")]
	public extern void CopyCounterValue(ComputeBuffer src, ComputeBuffer dst, uint dstOffsetBytes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("ClearCommands")]
	public extern void Clear();

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawMesh", HasExplicitThis = true)]
	private void Internal_DrawMesh([NotNull] Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties)
	{
		Internal_DrawMesh_Injected(mesh, ref matrix, material, submeshIndex, shaderPass, properties);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddDrawRenderer")]
	private extern void Internal_DrawRenderer([NotNull] Renderer renderer, Material material, int submeshIndex, int shaderPass);

	private void Internal_DrawRenderer(Renderer renderer, Material material, int submeshIndex)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_DrawRenderer(renderer, material, submeshIndex, -1);
	}

	private void Internal_DrawRenderer(Renderer renderer, Material material)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_DrawRenderer(renderer, material, 0);
	}

	[NativeMethod("AddDrawProcedural")]
	private void Internal_DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		Internal_DrawProcedural_Injected(ref matrix, material, shaderPass, topology, vertexCount, instanceCount, properties);
	}

	[NativeMethod("AddDrawProceduralIndexed")]
	private void Internal_DrawProceduralIndexed(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		Internal_DrawProceduralIndexed_Injected(indexBuffer, ref matrix, material, shaderPass, topology, indexCount, instanceCount, properties);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawProceduralIndirect", HasExplicitThis = true)]
	private void Internal_DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		Internal_DrawProceduralIndirect_Injected(ref matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawProceduralIndexedIndirect", HasExplicitThis = true)]
	private void Internal_DrawProceduralIndexedIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		Internal_DrawProceduralIndexedIndirect_Injected(indexBuffer, ref matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawMeshInstanced", HasExplicitThis = true)]
	private extern void Internal_DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawMeshInstancedProcedural", HasExplicitThis = true)]
	private extern void Internal_DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawMeshInstancedIndirect", HasExplicitThis = true)]
	private extern void Internal_DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

	[FreeFunction("RenderingCommandBuffer_Bindings::Internal_DrawOcclusionMesh", HasExplicitThis = true)]
	private void Internal_DrawOcclusionMesh(RectInt normalizedCamViewport)
	{
		Internal_DrawOcclusionMesh_Injected(ref normalizedCamViewport);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetRandomWriteTarget_Texture", HasExplicitThis = true, ThrowsException = true)]
	private extern void SetRandomWriteTarget_Texture(int index, ref RenderTargetIdentifier rt);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetRandomWriteTarget_Buffer", HasExplicitThis = true, ThrowsException = true)]
	private extern void SetRandomWriteTarget_Buffer(int index, ComputeBuffer uav, bool preserveCounterValue);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::ClearRandomWriteTargets", HasExplicitThis = true, ThrowsException = true)]
	public extern void ClearRandomWriteTargets();

	[FreeFunction("RenderingCommandBuffer_Bindings::SetViewport", HasExplicitThis = true, ThrowsException = true)]
	public void SetViewport(Rect pixelRect)
	{
		SetViewport_Injected(ref pixelRect);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::EnableScissorRect", HasExplicitThis = true, ThrowsException = true)]
	public void EnableScissorRect(Rect scissor)
	{
		EnableScissorRect_Injected(ref scissor);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::DisableScissorRect", HasExplicitThis = true, ThrowsException = true)]
	public extern void DisableScissorRect();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::CopyTexture_Internal", HasExplicitThis = true)]
	private extern void CopyTexture_Internal(ref RenderTargetIdentifier src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, ref RenderTargetIdentifier dst, int dstElement, int dstMip, int dstX, int dstY, int mode);

	[FreeFunction("RenderingCommandBuffer_Bindings::Blit_Texture", HasExplicitThis = true)]
	private void Blit_Texture(Texture source, ref RenderTargetIdentifier dest, Material mat, int pass, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice)
	{
		Blit_Texture_Injected(source, ref dest, mat, pass, ref scale, ref offset, sourceDepthSlice, destDepthSlice);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::Blit_Identifier", HasExplicitThis = true)]
	private void Blit_Identifier(ref RenderTargetIdentifier source, ref RenderTargetIdentifier dest, Material mat, int pass, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice)
	{
		Blit_Identifier_Injected(ref source, ref dest, mat, pass, ref scale, ref offset, sourceDepthSlice, destDepthSlice);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::GetTemporaryRT", HasExplicitThis = true)]
	public extern void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing, bool enableRandomWrite, RenderTextureMemoryless memorylessMode, bool useDynamicScale);

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing, bool enableRandomWrite, RenderTextureMemoryless memorylessMode)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, antiAliasing, enableRandomWrite, memorylessMode, useDynamicScale: false);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing, bool enableRandomWrite)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, antiAliasing, enableRandomWrite, RenderTextureMemoryless.None);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, antiAliasing, enableRandomWrite: false, RenderTextureMemoryless.None);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, GraphicsFormat format)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, 1);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing, bool enableRandomWrite, RenderTextureMemoryless memorylessMode, bool useDynamicScale)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, readWrite), antiAliasing, enableRandomWrite, memorylessMode, useDynamicScale);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing, bool enableRandomWrite, RenderTextureMemoryless memorylessMode)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, readWrite, antiAliasing, enableRandomWrite, memorylessMode, useDynamicScale: false);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing, bool enableRandomWrite)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, readWrite, antiAliasing, enableRandomWrite, RenderTextureMemoryless.None);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, readWrite, antiAliasing, enableRandomWrite: false);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, format, readWrite, 1);
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter, RenderTextureFormat format)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, RenderTextureReadWrite.Default));
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer, FilterMode filter)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, filter, SystemInfo.GetGraphicsFormat(DefaultFormat.LDR));
	}

	public void GetTemporaryRT(int nameID, int width, int height, int depthBuffer)
	{
		GetTemporaryRT(nameID, width, height, depthBuffer, FilterMode.Point);
	}

	public void GetTemporaryRT(int nameID, int width, int height)
	{
		GetTemporaryRT(nameID, width, height, 0);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::GetTemporaryRTWithDescriptor", HasExplicitThis = true)]
	private void GetTemporaryRTWithDescriptor(int nameID, RenderTextureDescriptor desc, FilterMode filter)
	{
		GetTemporaryRTWithDescriptor_Injected(nameID, ref desc, filter);
	}

	public void GetTemporaryRT(int nameID, RenderTextureDescriptor desc, FilterMode filter)
	{
		GetTemporaryRTWithDescriptor(nameID, desc, filter);
	}

	public void GetTemporaryRT(int nameID, RenderTextureDescriptor desc)
	{
		GetTemporaryRT(nameID, desc, FilterMode.Point);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::GetTemporaryRTArray", HasExplicitThis = true)]
	public extern void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing, bool enableRandomWrite, bool useDynamicScale);

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing, bool enableRandomWrite)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, format, antiAliasing, enableRandomWrite, useDynamicScale: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, GraphicsFormat format, int antiAliasing)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, format, antiAliasing, enableRandomWrite: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, GraphicsFormat format)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, format, 1);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing, bool enableRandomWrite)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, readWrite), antiAliasing, enableRandomWrite, useDynamicScale: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite, int antiAliasing)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, readWrite), antiAliasing, enableRandomWrite: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, readWrite), 1, enableRandomWrite: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter, RenderTextureFormat format)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, GraphicsFormatUtility.GetGraphicsFormat(format, RenderTextureReadWrite.Default), 1, enableRandomWrite: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer, FilterMode filter)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, filter, SystemInfo.GetGraphicsFormat(DefaultFormat.LDR), 1, enableRandomWrite: false);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices, int depthBuffer)
	{
		GetTemporaryRTArray(nameID, width, height, slices, depthBuffer, FilterMode.Point);
	}

	public void GetTemporaryRTArray(int nameID, int width, int height, int slices)
	{
		GetTemporaryRTArray(nameID, width, height, slices, 0);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::ReleaseTemporaryRT", HasExplicitThis = true)]
	public extern void ReleaseTemporaryRT(int nameID);

	[FreeFunction("RenderingCommandBuffer_Bindings::ClearRenderTarget", HasExplicitThis = true)]
	public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth)
	{
		ClearRenderTarget_Injected(clearDepth, clearColor, ref backgroundColor, depth);
	}

	public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		ClearRenderTarget(clearDepth, clearColor, backgroundColor, 1f);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalFloat", HasExplicitThis = true)]
	public extern void SetGlobalFloat(int nameID, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalInt", HasExplicitThis = true)]
	public extern void SetGlobalInt(int nameID, int value);

	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalVector", HasExplicitThis = true)]
	public void SetGlobalVector(int nameID, Vector4 value)
	{
		SetGlobalVector_Injected(nameID, ref value);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalColor", HasExplicitThis = true)]
	public void SetGlobalColor(int nameID, Color value)
	{
		SetGlobalColor_Injected(nameID, ref value);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalMatrix", HasExplicitThis = true)]
	public void SetGlobalMatrix(int nameID, Matrix4x4 value)
	{
		SetGlobalMatrix_Injected(nameID, ref value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::EnableShaderKeyword", HasExplicitThis = true)]
	public extern void EnableShaderKeyword(string keyword);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::DisableShaderKeyword", HasExplicitThis = true)]
	public extern void DisableShaderKeyword(string keyword);

	[FreeFunction("RenderingCommandBuffer_Bindings::SetViewMatrix", HasExplicitThis = true, ThrowsException = true)]
	public void SetViewMatrix(Matrix4x4 view)
	{
		SetViewMatrix_Injected(ref view);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::SetProjectionMatrix", HasExplicitThis = true, ThrowsException = true)]
	public void SetProjectionMatrix(Matrix4x4 proj)
	{
		SetProjectionMatrix_Injected(ref proj);
	}

	[FreeFunction("RenderingCommandBuffer_Bindings::SetViewProjectionMatrices", HasExplicitThis = true, ThrowsException = true)]
	public void SetViewProjectionMatrices(Matrix4x4 view, Matrix4x4 proj)
	{
		SetViewProjectionMatrices_Injected(ref view, ref proj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("AddSetGlobalDepthBias")]
	public extern void SetGlobalDepthBias(float bias, float slopeBias);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetExecutionFlags", HasExplicitThis = true, ThrowsException = true)]
	public extern void SetExecutionFlags(CommandBufferExecutionFlags flags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::ValidateAgainstExecutionFlags", HasExplicitThis = true, ThrowsException = true)]
	private extern bool ValidateAgainstExecutionFlags(CommandBufferExecutionFlags requiredFlags, CommandBufferExecutionFlags invalidFlags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalFloatArrayListImpl", HasExplicitThis = true)]
	private extern void SetGlobalFloatArrayListImpl(int nameID, object values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalVectorArrayListImpl", HasExplicitThis = true)]
	private extern void SetGlobalVectorArrayListImpl(int nameID, object values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalMatrixArrayListImpl", HasExplicitThis = true)]
	private extern void SetGlobalMatrixArrayListImpl(int nameID, object values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalFloatArray", HasExplicitThis = true, ThrowsException = true)]
	public extern void SetGlobalFloatArray(int nameID, float[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalVectorArray", HasExplicitThis = true, ThrowsException = true)]
	public extern void SetGlobalVectorArray(int nameID, Vector4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalMatrixArray", HasExplicitThis = true, ThrowsException = true)]
	public extern void SetGlobalMatrixArray(int nameID, Matrix4x4[] values);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalTexture_Impl", HasExplicitThis = true)]
	private extern void SetGlobalTexture_Impl(int nameID, ref RenderTargetIdentifier rt, RenderTextureSubElement element);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalBuffer", HasExplicitThis = true)]
	public extern void SetGlobalBuffer(int nameID, ComputeBuffer value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetShadowSamplingMode_Impl", HasExplicitThis = true)]
	private extern void SetShadowSamplingMode_Impl(ref RenderTargetIdentifier shadowmap, ShadowSamplingMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::IssuePluginEventInternal", HasExplicitThis = true)]
	private extern void IssuePluginEventInternal(IntPtr callback, int eventID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::BeginSample", HasExplicitThis = true)]
	public extern void BeginSample(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::EndSample", HasExplicitThis = true)]
	public extern void EndSample(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::IssuePluginEventAndDataInternal", HasExplicitThis = true)]
	private extern void IssuePluginEventAndDataInternal(IntPtr callback, int eventID, IntPtr data);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::IssuePluginCustomBlitInternal", HasExplicitThis = true)]
	private extern void IssuePluginCustomBlitInternal(IntPtr callback, uint command, ref RenderTargetIdentifier source, ref RenderTargetIdentifier dest, uint commandParam, uint commandFlags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::IssuePluginCustomTextureUpdateInternal", HasExplicitThis = true)]
	private extern void IssuePluginCustomTextureUpdateInternal(IntPtr callback, Texture targetTexture, uint userData, bool useNewUnityRenderingExtTextureUpdateParamsV2);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetGlobalConstantBuffer", HasExplicitThis = true)]
	public extern void SetGlobalConstantBuffer(ComputeBuffer buffer, int nameID, int offset, int size);

	[FreeFunction("RenderingCommandBuffer_Bindings::IncrementUpdateCount", HasExplicitThis = true)]
	public void IncrementUpdateCount(RenderTargetIdentifier dest)
	{
		IncrementUpdateCount_Injected(ref dest);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RenderingCommandBuffer_Bindings::SetInstanceMultiplier", HasExplicitThis = true)]
	public extern void SetInstanceMultiplier(uint multiplier);

	public void SetRenderTarget(RenderTargetIdentifier rt)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		SetRenderTargetSingle_Internal(rt, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier rt, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (loadAction == RenderBufferLoadAction.Clear)
		{
			throw new ArgumentException("RenderBufferLoadAction.Clear is not supported");
		}
		SetRenderTargetSingle_Internal(rt, loadAction, storeAction, loadAction, storeAction);
	}

	public void SetRenderTarget(RenderTargetIdentifier rt, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (colorLoadAction == RenderBufferLoadAction.Clear || depthLoadAction == RenderBufferLoadAction.Clear)
		{
			throw new ArgumentException("RenderBufferLoadAction.Clear is not supported");
		}
		SetRenderTargetSingle_Internal(rt, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
	}

	public void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetSingle_Internal(new RenderTargetIdentifier(rt, mipLevel), RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel, CubemapFace cubemapFace)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetSingle_Internal(new RenderTargetIdentifier(rt, mipLevel, cubemapFace), RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (depthSlice < -1)
		{
			throw new ArgumentException($"Invalid value for depthSlice ({depthSlice})");
		}
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetSingle_Internal(new RenderTargetIdentifier(rt, mipLevel, cubemapFace, depthSlice), RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		SetRenderTargetColorDepth_Internal(color, depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetColorDepth_Internal(new RenderTargetIdentifier(color, mipLevel), depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetColorDepth_Internal(new RenderTargetIdentifier(color, mipLevel, cubemapFace), depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (depthSlice < -1)
		{
			throw new ArgumentException($"Invalid value for depthSlice ({depthSlice})");
		}
		if (mipLevel < 0)
		{
			throw new ArgumentException($"Invalid value for mipLevel ({mipLevel})");
		}
		SetRenderTargetColorDepth_Internal(new RenderTargetIdentifier(color, mipLevel, cubemapFace, depthSlice), depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier color, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderTargetIdentifier depth, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (colorLoadAction == RenderBufferLoadAction.Clear || depthLoadAction == RenderBufferLoadAction.Clear)
		{
			throw new ArgumentException("RenderBufferLoadAction.Clear is not supported");
		}
		SetRenderTargetColorDepth_Internal(color, depth, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
	}

	public void SetRenderTarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (colors.Length < 1)
		{
			throw new ArgumentException($"colors.Length must be at least 1, but was {colors.Length}");
		}
		if (colors.Length > SystemInfo.supportedRenderTargetCount)
		{
			throw new ArgumentException($"colors.Length is {colors.Length} and exceeds the maximum number of supported render targets ({SystemInfo.supportedRenderTargetCount})");
		}
		SetRenderTargetMulti_Internal(colors, depth, null, null, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
	}

	public void SetRenderTarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (colors.Length < 1)
		{
			throw new ArgumentException($"colors.Length must be at least 1, but was {colors.Length}");
		}
		if (colors.Length > SystemInfo.supportedRenderTargetCount)
		{
			throw new ArgumentException($"colors.Length is {colors.Length} and exceeds the maximum number of supported render targets ({SystemInfo.supportedRenderTargetCount})");
		}
		SetRenderTargetMultiSubtarget(colors, depth, null, null, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, mipLevel, cubemapFace, depthSlice);
	}

	public void SetRenderTarget(RenderTargetBinding binding, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (binding.colorRenderTargets.Length < 1)
		{
			throw new ArgumentException($"The number of color render targets must be at least 1, but was {binding.colorRenderTargets.Length}");
		}
		if (binding.colorRenderTargets.Length > SystemInfo.supportedRenderTargetCount)
		{
			throw new ArgumentException($"The number of color render targets ({binding.colorRenderTargets.Length}) and exceeds the maximum supported number of render targets ({SystemInfo.supportedRenderTargetCount})");
		}
		if (binding.colorLoadActions.Length != binding.colorRenderTargets.Length)
		{
			throw new ArgumentException($"The number of color load actions provided ({binding.colorLoadActions.Length}) does not match the number of color render targets ({binding.colorRenderTargets.Length})");
		}
		if (binding.colorStoreActions.Length != binding.colorRenderTargets.Length)
		{
			throw new ArgumentException($"The number of color store actions provided ({binding.colorLoadActions.Length}) does not match the number of color render targets ({binding.colorRenderTargets.Length})");
		}
		if (binding.depthLoadAction == RenderBufferLoadAction.Clear || Array.IndexOf(binding.colorLoadActions, RenderBufferLoadAction.Clear) > -1)
		{
			throw new ArgumentException("RenderBufferLoadAction.Clear is not supported");
		}
		if (binding.colorRenderTargets.Length == 1)
		{
			SetRenderTargetColorDepthSubtarget(binding.colorRenderTargets[0], binding.depthRenderTarget, binding.colorLoadActions[0], binding.colorStoreActions[0], binding.depthLoadAction, binding.depthStoreAction, mipLevel, cubemapFace, depthSlice);
		}
		else
		{
			SetRenderTargetMultiSubtarget(binding.colorRenderTargets, binding.depthRenderTarget, binding.colorLoadActions, binding.colorStoreActions, binding.depthLoadAction, binding.depthStoreAction, mipLevel, cubemapFace, depthSlice);
		}
	}

	public void SetRenderTarget(RenderTargetBinding binding)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (binding.colorRenderTargets.Length < 1)
		{
			throw new ArgumentException($"The number of color render targets must be at least 1, but was {binding.colorRenderTargets.Length}");
		}
		if (binding.colorRenderTargets.Length > SystemInfo.supportedRenderTargetCount)
		{
			throw new ArgumentException($"The number of color render targets ({binding.colorRenderTargets.Length}) and exceeds the maximum supported number of render targets ({SystemInfo.supportedRenderTargetCount})");
		}
		if (binding.colorLoadActions.Length != binding.colorRenderTargets.Length)
		{
			throw new ArgumentException($"The number of color load actions provided ({binding.colorLoadActions.Length}) does not match the number of color render targets ({binding.colorRenderTargets.Length})");
		}
		if (binding.colorStoreActions.Length != binding.colorRenderTargets.Length)
		{
			throw new ArgumentException($"The number of color store actions provided ({binding.colorLoadActions.Length}) does not match the number of color render targets ({binding.colorRenderTargets.Length})");
		}
		if (binding.depthLoadAction == RenderBufferLoadAction.Clear || Array.IndexOf(binding.colorLoadActions, RenderBufferLoadAction.Clear) > -1)
		{
			throw new ArgumentException("RenderBufferLoadAction.Clear is not supported");
		}
		if (binding.colorRenderTargets.Length == 1)
		{
			SetRenderTargetColorDepth_Internal(binding.colorRenderTargets[0], binding.depthRenderTarget, binding.colorLoadActions[0], binding.colorStoreActions[0], binding.depthLoadAction, binding.depthStoreAction);
		}
		else
		{
			SetRenderTargetMulti_Internal(binding.colorRenderTargets, binding.depthRenderTarget, binding.colorLoadActions, binding.colorStoreActions, binding.depthLoadAction, binding.depthStoreAction);
		}
	}

	private void SetRenderTargetSingle_Internal(RenderTargetIdentifier rt, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
	{
		SetRenderTargetSingle_Internal_Injected(ref rt, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
	}

	private void SetRenderTargetColorDepth_Internal(RenderTargetIdentifier color, RenderTargetIdentifier depth, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
	{
		SetRenderTargetColorDepth_Internal_Injected(ref color, ref depth, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
	}

	private void SetRenderTargetMulti_Internal(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth, RenderBufferLoadAction[] colorLoadActions, RenderBufferStoreAction[] colorStoreActions, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
	{
		SetRenderTargetMulti_Internal_Injected(colors, ref depth, colorLoadActions, colorStoreActions, depthLoadAction, depthStoreAction);
	}

	private void SetRenderTargetColorDepthSubtarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		SetRenderTargetColorDepthSubtarget_Injected(ref color, ref depth, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction, mipLevel, cubemapFace, depthSlice);
	}

	private void SetRenderTargetMultiSubtarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth, RenderBufferLoadAction[] colorLoadActions, RenderBufferStoreAction[] colorStoreActions, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, int mipLevel, CubemapFace cubemapFace, int depthSlice)
	{
		SetRenderTargetMultiSubtarget_Injected(colors, ref depth, colorLoadActions, colorStoreActions, depthLoadAction, depthStoreAction, mipLevel, cubemapFace, depthSlice);
	}

	~CommandBuffer()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		ReleaseBuffer();
		m_Ptr = IntPtr.Zero;
	}

	public CommandBuffer()
	{
		m_Ptr = InitBuffer();
	}

	public void Release()
	{
		Dispose();
	}

	public GraphicsFence CreateAsyncGraphicsFence()
	{
		return CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation, SynchronisationStageFlags.PixelProcessing);
	}

	public GraphicsFence CreateAsyncGraphicsFence(SynchronisationStage stage)
	{
		return CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation, GraphicsFence.TranslateSynchronizationStageToFlags(stage));
	}

	public GraphicsFence CreateGraphicsFence(GraphicsFenceType fenceType, SynchronisationStageFlags stage)
	{
		GraphicsFence result = default(GraphicsFence);
		result.m_Ptr = CreateGPUFence_Internal(fenceType, stage);
		result.InitPostAllocation();
		result.Validate();
		return result;
	}

	public void WaitOnAsyncGraphicsFence(GraphicsFence fence)
	{
		WaitOnAsyncGraphicsFence(fence, SynchronisationStage.VertexProcessing);
	}

	public void WaitOnAsyncGraphicsFence(GraphicsFence fence, SynchronisationStage stage)
	{
		WaitOnAsyncGraphicsFence(fence, GraphicsFence.TranslateSynchronizationStageToFlags(stage));
	}

	public void WaitOnAsyncGraphicsFence(GraphicsFence fence, SynchronisationStageFlags stage)
	{
		if (fence.m_FenceType != 0)
		{
			throw new ArgumentException("Attempting to call WaitOnAsyncGPUFence on a fence that is not of GraphicsFenceType.AsyncQueueSynchronization");
		}
		fence.Validate();
		if (fence.IsFencePending())
		{
			WaitOnGPUFence_Internal(fence.m_Ptr, stage);
		}
	}

	public void SetComputeFloatParam(ComputeShader computeShader, string name, float val)
	{
		SetComputeFloatParam(computeShader, Shader.PropertyToID(name), val);
	}

	public void SetComputeIntParam(ComputeShader computeShader, string name, int val)
	{
		SetComputeIntParam(computeShader, Shader.PropertyToID(name), val);
	}

	public void SetComputeVectorParam(ComputeShader computeShader, string name, Vector4 val)
	{
		SetComputeVectorParam(computeShader, Shader.PropertyToID(name), val);
	}

	public void SetComputeVectorArrayParam(ComputeShader computeShader, string name, Vector4[] values)
	{
		SetComputeVectorArrayParam(computeShader, Shader.PropertyToID(name), values);
	}

	public void SetComputeMatrixParam(ComputeShader computeShader, string name, Matrix4x4 val)
	{
		SetComputeMatrixParam(computeShader, Shader.PropertyToID(name), val);
	}

	public void SetComputeMatrixArrayParam(ComputeShader computeShader, string name, Matrix4x4[] values)
	{
		SetComputeMatrixArrayParam(computeShader, Shader.PropertyToID(name), values);
	}

	public void SetComputeFloatParams(ComputeShader computeShader, string name, params float[] values)
	{
		Internal_SetComputeFloats(computeShader, Shader.PropertyToID(name), values);
	}

	public void SetComputeFloatParams(ComputeShader computeShader, int nameID, params float[] values)
	{
		Internal_SetComputeFloats(computeShader, nameID, values);
	}

	public void SetComputeIntParams(ComputeShader computeShader, string name, params int[] values)
	{
		Internal_SetComputeInts(computeShader, Shader.PropertyToID(name), values);
	}

	public void SetComputeIntParams(ComputeShader computeShader, int nameID, params int[] values)
	{
		Internal_SetComputeInts(computeShader, nameID, values);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, RenderTargetIdentifier rt)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, Shader.PropertyToID(name), ref rt, 0, RenderTextureSubElement.Default);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, RenderTargetIdentifier rt)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, nameID, ref rt, 0, RenderTextureSubElement.Default);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, RenderTargetIdentifier rt, int mipLevel)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, Shader.PropertyToID(name), ref rt, mipLevel, RenderTextureSubElement.Default);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, RenderTargetIdentifier rt, int mipLevel)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, nameID, ref rt, mipLevel, RenderTextureSubElement.Default);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, string name, RenderTargetIdentifier rt, int mipLevel, RenderTextureSubElement element)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, Shader.PropertyToID(name), ref rt, mipLevel, element);
	}

	public void SetComputeTextureParam(ComputeShader computeShader, int kernelIndex, int nameID, RenderTargetIdentifier rt, int mipLevel, RenderTextureSubElement element)
	{
		Internal_SetComputeTextureParam(computeShader, kernelIndex, nameID, ref rt, mipLevel, element);
	}

	public void SetComputeBufferParam(ComputeShader computeShader, int kernelIndex, string name, ComputeBuffer buffer)
	{
		SetComputeBufferParam(computeShader, kernelIndex, Shader.PropertyToID(name), buffer);
	}

	public void DispatchCompute(ComputeShader computeShader, int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ)
	{
		Internal_DispatchCompute(computeShader, kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
	}

	public void DispatchCompute(ComputeShader computeShader, int kernelIndex, ComputeBuffer indirectBuffer, uint argsOffset)
	{
		Internal_DispatchComputeIndirect(computeShader, kernelIndex, indirectBuffer, argsOffset);
	}

	public void BuildRayTracingAccelerationStructure(RayTracingAccelerationStructure accelerationStructure)
	{
		Internal_BuildRayTracingAccelerationStructure(accelerationStructure);
	}

	public void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, string name, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		Internal_SetRayTracingAccelerationStructure(rayTracingShader, Shader.PropertyToID(name), rayTracingAccelerationStructure);
	}

	public void SetRayTracingAccelerationStructure(RayTracingShader rayTracingShader, int nameID, RayTracingAccelerationStructure rayTracingAccelerationStructure)
	{
		Internal_SetRayTracingAccelerationStructure(rayTracingShader, nameID, rayTracingAccelerationStructure);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, string name, ComputeBuffer buffer)
	{
		Internal_SetRayTracingBufferParam(rayTracingShader, Shader.PropertyToID(name), buffer);
	}

	public void SetRayTracingBufferParam(RayTracingShader rayTracingShader, int nameID, ComputeBuffer buffer)
	{
		Internal_SetRayTracingBufferParam(rayTracingShader, nameID, buffer);
	}

	public void SetRayTracingTextureParam(RayTracingShader rayTracingShader, string name, RenderTargetIdentifier rt)
	{
		Internal_SetRayTracingTextureParam(rayTracingShader, Shader.PropertyToID(name), ref rt);
	}

	public void SetRayTracingTextureParam(RayTracingShader rayTracingShader, int nameID, RenderTargetIdentifier rt)
	{
		Internal_SetRayTracingTextureParam(rayTracingShader, nameID, ref rt);
	}

	public void SetRayTracingFloatParam(RayTracingShader rayTracingShader, string name, float val)
	{
		Internal_SetRayTracingFloatParam(rayTracingShader, Shader.PropertyToID(name), val);
	}

	public void SetRayTracingFloatParam(RayTracingShader rayTracingShader, int nameID, float val)
	{
		Internal_SetRayTracingFloatParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingFloatParams(RayTracingShader rayTracingShader, string name, params float[] values)
	{
		Internal_SetRayTracingFloats(rayTracingShader, Shader.PropertyToID(name), values);
	}

	public void SetRayTracingFloatParams(RayTracingShader rayTracingShader, int nameID, params float[] values)
	{
		Internal_SetRayTracingFloats(rayTracingShader, nameID, values);
	}

	public void SetRayTracingIntParam(RayTracingShader rayTracingShader, string name, int val)
	{
		Internal_SetRayTracingIntParam(rayTracingShader, Shader.PropertyToID(name), val);
	}

	public void SetRayTracingIntParam(RayTracingShader rayTracingShader, int nameID, int val)
	{
		Internal_SetRayTracingIntParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingIntParams(RayTracingShader rayTracingShader, string name, params int[] values)
	{
		Internal_SetRayTracingInts(rayTracingShader, Shader.PropertyToID(name), values);
	}

	public void SetRayTracingIntParams(RayTracingShader rayTracingShader, int nameID, params int[] values)
	{
		Internal_SetRayTracingInts(rayTracingShader, nameID, values);
	}

	public void SetRayTracingVectorParam(RayTracingShader rayTracingShader, string name, Vector4 val)
	{
		Internal_SetRayTracingVectorParam(rayTracingShader, Shader.PropertyToID(name), val);
	}

	public void SetRayTracingVectorParam(RayTracingShader rayTracingShader, int nameID, Vector4 val)
	{
		Internal_SetRayTracingVectorParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, string name, params Vector4[] values)
	{
		Internal_SetRayTracingVectorArrayParam(rayTracingShader, Shader.PropertyToID(name), values);
	}

	public void SetRayTracingVectorArrayParam(RayTracingShader rayTracingShader, int nameID, params Vector4[] values)
	{
		Internal_SetRayTracingVectorArrayParam(rayTracingShader, nameID, values);
	}

	public void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, string name, Matrix4x4 val)
	{
		Internal_SetRayTracingMatrixParam(rayTracingShader, Shader.PropertyToID(name), val);
	}

	public void SetRayTracingMatrixParam(RayTracingShader rayTracingShader, int nameID, Matrix4x4 val)
	{
		Internal_SetRayTracingMatrixParam(rayTracingShader, nameID, val);
	}

	public void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, string name, params Matrix4x4[] values)
	{
		Internal_SetRayTracingMatrixArrayParam(rayTracingShader, Shader.PropertyToID(name), values);
	}

	public void SetRayTracingMatrixArrayParam(RayTracingShader rayTracingShader, int nameID, params Matrix4x4[] values)
	{
		Internal_SetRayTracingMatrixArrayParam(rayTracingShader, nameID, values);
	}

	public void DispatchRays(RayTracingShader rayTracingShader, string rayGenName, uint width, uint height, uint depth, Camera camera = null)
	{
		Internal_DispatchRays(rayTracingShader, rayGenName, width, height, depth, camera);
	}

	public void GenerateMips(RenderTexture rt)
	{
		if (rt == null)
		{
			throw new ArgumentNullException("rt");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_GenerateMips(rt);
	}

	public void ResolveAntiAliasedSurface(RenderTexture rt, RenderTexture target = null)
	{
		if (rt == null)
		{
			throw new ArgumentNullException("rt");
		}
		Internal_ResolveAntiAliasedSurface(rt, target);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
		{
			submeshIndex = Mathf.Clamp(submeshIndex, 0, mesh.subMeshCount - 1);
			Debug.LogWarning($"submeshIndex out of range. Clampped to {submeshIndex}.");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		Internal_DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass)
	{
		DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, null);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex)
	{
		DrawMesh(mesh, matrix, material, submeshIndex, -1);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material)
	{
		DrawMesh(mesh, matrix, material, 0);
	}

	public void DrawRenderer(Renderer renderer, Material material, int submeshIndex, int shaderPass)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (submeshIndex < 0)
		{
			submeshIndex = Mathf.Max(submeshIndex, 0);
			Debug.LogWarning($"submeshIndex out of range. Clampped to {submeshIndex}.");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		Internal_DrawRenderer(renderer, material, submeshIndex, shaderPass);
	}

	public void DrawRenderer(Renderer renderer, Material material, int submeshIndex)
	{
		DrawRenderer(renderer, material, submeshIndex, -1);
	}

	public void DrawRenderer(Renderer renderer, Material material)
	{
		DrawRenderer(renderer, material, 0);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount, properties);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount)
	{
		DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount, null);
	}

	public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount)
	{
		DrawProcedural(matrix, material, shaderPass, topology, vertexCount, 1);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties)
	{
		if (indexBuffer == null)
		{
			throw new ArgumentNullException("indexBuffer");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		Internal_DrawProceduralIndexed(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount, properties);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount)
	{
		DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount, null);
	}

	public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount)
	{
		DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, 1);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		if (bufferWithArgs == null)
		{
			throw new ArgumentNullException("bufferWithArgs");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Internal_DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, null);
	}

	public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
	{
		DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, 0);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		if (indexBuffer == null)
		{
			throw new ArgumentNullException("indexBuffer");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		if (bufferWithArgs == null)
		{
			throw new ArgumentNullException("bufferWithArgs");
		}
		Internal_DrawProceduralIndexedIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, null);
	}

	public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
	{
		DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, 0);
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties)
	{
		if (!SystemInfo.supportsInstancing)
		{
			throw new InvalidOperationException("DrawMeshInstanced is not supported.");
		}
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
		{
			throw new ArgumentOutOfRangeException("submeshIndex", "submeshIndex out of range.");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		if (matrices == null)
		{
			throw new ArgumentNullException("matrices");
		}
		if (count < 0 || count > Mathf.Min(Graphics.kMaxDrawMeshInstanceCount, matrices.Length))
		{
			throw new ArgumentOutOfRangeException("count", $"Count must be in the range of 0 to {Mathf.Min(Graphics.kMaxDrawMeshInstanceCount, matrices.Length)}.");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (count > 0)
		{
			Internal_DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count, properties);
		}
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count)
	{
		DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count, null);
	}

	public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices)
	{
		DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, matrices.Length);
	}

	public void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties = null)
	{
		if (!SystemInfo.supportsInstancing)
		{
			throw new InvalidOperationException("DrawMeshInstancedProcedural is not supported.");
		}
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
		{
			throw new ArgumentOutOfRangeException("submeshIndex", "submeshIndex out of range.");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		if (count > 0)
		{
			Internal_DrawMeshInstancedProcedural(mesh, submeshIndex, material, shaderPass, count, properties);
		}
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
	{
		if (!SystemInfo.supportsInstancing)
		{
			throw new InvalidOperationException("Instancing is not supported.");
		}
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
		{
			throw new ArgumentOutOfRangeException("submeshIndex", "submeshIndex out of range.");
		}
		if (material == null)
		{
			throw new ArgumentNullException("material");
		}
		if (bufferWithArgs == null)
		{
			throw new ArgumentNullException("bufferWithArgs");
		}
		Internal_DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, properties);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset)
	{
		DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, null);
	}

	public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs)
	{
		DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, 0, null);
	}

	public void DrawOcclusionMesh(RectInt normalizedCamViewport)
	{
		Internal_DrawOcclusionMesh(normalizedCamViewport);
	}

	public void SetRandomWriteTarget(int index, RenderTargetIdentifier rt)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		SetRandomWriteTarget_Texture(index, ref rt);
	}

	public void SetRandomWriteTarget(int index, ComputeBuffer buffer, bool preserveCounterValue)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		SetRandomWriteTarget_Buffer(index, buffer, preserveCounterValue);
	}

	public void SetRandomWriteTarget(int index, ComputeBuffer buffer)
	{
		SetRandomWriteTarget(index, buffer, preserveCounterValue: false);
	}

	public void CopyTexture(RenderTargetIdentifier src, RenderTargetIdentifier dst)
	{
		CopyTexture_Internal(ref src, -1, -1, -1, -1, -1, -1, ref dst, -1, -1, -1, -1, 1);
	}

	public void CopyTexture(RenderTargetIdentifier src, int srcElement, RenderTargetIdentifier dst, int dstElement)
	{
		CopyTexture_Internal(ref src, srcElement, -1, -1, -1, -1, -1, ref dst, dstElement, -1, -1, -1, 2);
	}

	public void CopyTexture(RenderTargetIdentifier src, int srcElement, int srcMip, RenderTargetIdentifier dst, int dstElement, int dstMip)
	{
		CopyTexture_Internal(ref src, srcElement, srcMip, -1, -1, -1, -1, ref dst, dstElement, dstMip, -1, -1, 3);
	}

	public void CopyTexture(RenderTargetIdentifier src, int srcElement, int srcMip, int srcX, int srcY, int srcWidth, int srcHeight, RenderTargetIdentifier dst, int dstElement, int dstMip, int dstX, int dstY)
	{
		CopyTexture_Internal(ref src, srcElement, srcMip, srcX, srcY, srcWidth, srcHeight, ref dst, dstElement, dstMip, dstX, dstY, 4);
	}

	public void Blit(Texture source, RenderTargetIdentifier dest)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Texture(source, ref dest, null, -1, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(Texture source, RenderTargetIdentifier dest, Vector2 scale, Vector2 offset)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Texture(source, ref dest, null, -1, scale, offset, Texture2DArray.allSlices, 0);
	}

	public void Blit(Texture source, RenderTargetIdentifier dest, Material mat)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Texture(source, ref dest, mat, -1, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(Texture source, RenderTargetIdentifier dest, Material mat, int pass)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Texture(source, ref dest, mat, pass, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, null, -1, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Vector2 scale, Vector2 offset)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, null, -1, scale, offset, Texture2DArray.allSlices, 0);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, mat, -1, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, mat, pass, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, 0);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, int sourceDepthSlice, int destDepthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, null, -1, new Vector2(1f, 1f), new Vector2(0f, 0f), sourceDepthSlice, destDepthSlice);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, null, -1, scale, offset, sourceDepthSlice, destDepthSlice);
	}

	public void Blit(RenderTargetIdentifier source, RenderTargetIdentifier dest, Material mat, int pass, int destDepthSlice)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		Blit_Identifier(ref source, ref dest, mat, pass, new Vector2(1f, 1f), new Vector2(0f, 0f), Texture2DArray.allSlices, destDepthSlice);
	}

	public void SetGlobalFloat(string name, float value)
	{
		SetGlobalFloat(Shader.PropertyToID(name), value);
	}

	public void SetGlobalInt(string name, int value)
	{
		SetGlobalInt(Shader.PropertyToID(name), value);
	}

	public void SetGlobalVector(string name, Vector4 value)
	{
		SetGlobalVector(Shader.PropertyToID(name), value);
	}

	public void SetGlobalColor(string name, Color value)
	{
		SetGlobalColor(Shader.PropertyToID(name), value);
	}

	public void SetGlobalMatrix(string name, Matrix4x4 value)
	{
		SetGlobalMatrix(Shader.PropertyToID(name), value);
	}

	public void SetGlobalFloatArray(string propertyName, List<float> values)
	{
		SetGlobalFloatArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalFloatArray(int nameID, List<float> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Count == 0)
		{
			throw new ArgumentException("Zero-sized array is not allowed.");
		}
		SetGlobalFloatArrayListImpl(nameID, values);
	}

	public void SetGlobalFloatArray(string propertyName, float[] values)
	{
		SetGlobalFloatArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalVectorArray(string propertyName, List<Vector4> values)
	{
		SetGlobalVectorArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalVectorArray(int nameID, List<Vector4> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Count == 0)
		{
			throw new ArgumentException("Zero-sized array is not allowed.");
		}
		SetGlobalVectorArrayListImpl(nameID, values);
	}

	public void SetGlobalVectorArray(string propertyName, Vector4[] values)
	{
		SetGlobalVectorArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalMatrixArray(string propertyName, List<Matrix4x4> values)
	{
		SetGlobalMatrixArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalMatrixArray(int nameID, List<Matrix4x4> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Count == 0)
		{
			throw new ArgumentException("Zero-sized array is not allowed.");
		}
		SetGlobalMatrixArrayListImpl(nameID, values);
	}

	public void SetGlobalMatrixArray(string propertyName, Matrix4x4[] values)
	{
		SetGlobalMatrixArray(Shader.PropertyToID(propertyName), values);
	}

	public void SetGlobalTexture(string name, RenderTargetIdentifier value)
	{
		SetGlobalTexture(Shader.PropertyToID(name), value, RenderTextureSubElement.Default);
	}

	public void SetGlobalTexture(int nameID, RenderTargetIdentifier value)
	{
		SetGlobalTexture_Impl(nameID, ref value, RenderTextureSubElement.Default);
	}

	public void SetGlobalTexture(string name, RenderTargetIdentifier value, RenderTextureSubElement element)
	{
		SetGlobalTexture(Shader.PropertyToID(name), value, element);
	}

	public void SetGlobalTexture(int nameID, RenderTargetIdentifier value, RenderTextureSubElement element)
	{
		SetGlobalTexture_Impl(nameID, ref value, element);
	}

	public void SetGlobalBuffer(string name, ComputeBuffer value)
	{
		SetGlobalBuffer(Shader.PropertyToID(name), value);
	}

	public void SetShadowSamplingMode(RenderTargetIdentifier shadowmap, ShadowSamplingMode mode)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		SetShadowSamplingMode_Impl(ref shadowmap, mode);
	}

	public void SetSinglePassStereo(SinglePassStereoMode mode)
	{
		Internal_SetSinglePassStereo(mode);
	}

	public void IssuePluginEvent(IntPtr callback, int eventID)
	{
		if (callback == IntPtr.Zero)
		{
			throw new ArgumentException("Null callback specified.");
		}
		IssuePluginEventInternal(callback, eventID);
	}

	public void IssuePluginEventAndData(IntPtr callback, int eventID, IntPtr data)
	{
		if (callback == IntPtr.Zero)
		{
			throw new ArgumentException("Null callback specified.");
		}
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		IssuePluginEventAndDataInternal(callback, eventID, data);
	}

	public void IssuePluginCustomBlit(IntPtr callback, uint command, RenderTargetIdentifier source, RenderTargetIdentifier dest, uint commandParam, uint commandFlags)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		IssuePluginCustomBlitInternal(callback, command, ref source, ref dest, commandParam, commandFlags);
	}

	[Obsolete("Use IssuePluginCustomTextureUpdateV2 to register TextureUpdate callbacks instead. Callbacks will be passed event IDs kUnityRenderingExtEventUpdateTextureBeginV2 or kUnityRenderingExtEventUpdateTextureEndV2, and data parameter of type UnityRenderingExtTextureUpdateParamsV2.", false)]
	public void IssuePluginCustomTextureUpdate(IntPtr callback, Texture targetTexture, uint userData)
	{
		IssuePluginCustomTextureUpdateInternal(callback, targetTexture, userData, useNewUnityRenderingExtTextureUpdateParamsV2: false);
	}

	[Obsolete("Use IssuePluginCustomTextureUpdateV2 to register TextureUpdate callbacks instead. Callbacks will be passed event IDs kUnityRenderingExtEventUpdateTextureBeginV2 or kUnityRenderingExtEventUpdateTextureEndV2, and data parameter of type UnityRenderingExtTextureUpdateParamsV2.", false)]
	public void IssuePluginCustomTextureUpdateV1(IntPtr callback, Texture targetTexture, uint userData)
	{
		IssuePluginCustomTextureUpdateInternal(callback, targetTexture, userData, useNewUnityRenderingExtTextureUpdateParamsV2: false);
	}

	public void IssuePluginCustomTextureUpdateV2(IntPtr callback, Texture targetTexture, uint userData)
	{
		ValidateAgainstExecutionFlags(CommandBufferExecutionFlags.None, CommandBufferExecutionFlags.AsyncCompute);
		IssuePluginCustomTextureUpdateInternal(callback, targetTexture, userData, useNewUnityRenderingExtTextureUpdateParamsV2: true);
	}

	[Obsolete("CommandBuffer.CreateGPUFence has been deprecated. Use CreateGraphicsFence instead (UnityUpgradable) -> CreateAsyncGraphicsFence(*)", false)]
	public GPUFence CreateGPUFence(SynchronisationStage stage)
	{
		return default(GPUFence);
	}

	[Obsolete("CommandBuffer.CreateGPUFence has been deprecated. Use CreateGraphicsFence instead (UnityUpgradable) -> CreateAsyncGraphicsFence()", false)]
	public GPUFence CreateGPUFence()
	{
		return default(GPUFence);
	}

	[Obsolete("CommandBuffer.WaitOnGPUFence has been deprecated. Use WaitOnGraphicsFence instead (UnityUpgradable) -> WaitOnAsyncGraphicsFence(*)", false)]
	public void WaitOnGPUFence(GPUFence fence, SynchronisationStage stage)
	{
	}

	[Obsolete("CommandBuffer.WaitOnGPUFence has been deprecated. Use WaitOnGraphicsFence instead (UnityUpgradable) -> WaitOnAsyncGraphicsFence(*)", false)]
	public void WaitOnGPUFence(GPUFence fence)
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void ConvertTexture_Internal_Injected(ref RenderTargetIdentifier src, int srcElement, ref RenderTargetIdentifier dst, int dstElement);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetComputeVectorParam_Injected(ComputeShader computeShader, int nameID, ref Vector4 val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetComputeMatrixParam_Injected(ComputeShader computeShader, int nameID, ref Matrix4x4 val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_SetRayTracingVectorParam_Injected(RayTracingShader rayTracingShader, int nameID, ref Vector4 val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_SetRayTracingMatrixParam_Injected(RayTracingShader rayTracingShader, int nameID, ref Matrix4x4 val);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawMesh_Injected(Mesh mesh, ref Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawProcedural_Injected(ref Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawProceduralIndexed_Injected(GraphicsBuffer indexBuffer, ref Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawProceduralIndirect_Injected(ref Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawProceduralIndexedIndirect_Injected(GraphicsBuffer indexBuffer, ref Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_DrawOcclusionMesh_Injected(ref RectInt normalizedCamViewport);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetViewport_Injected(ref Rect pixelRect);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void EnableScissorRect_Injected(ref Rect scissor);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Blit_Texture_Injected(Texture source, ref RenderTargetIdentifier dest, Material mat, int pass, ref Vector2 scale, ref Vector2 offset, int sourceDepthSlice, int destDepthSlice);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Blit_Identifier_Injected(ref RenderTargetIdentifier source, ref RenderTargetIdentifier dest, Material mat, int pass, ref Vector2 scale, ref Vector2 offset, int sourceDepthSlice, int destDepthSlice);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetTemporaryRTWithDescriptor_Injected(int nameID, ref RenderTextureDescriptor desc, FilterMode filter);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void ClearRenderTarget_Injected(bool clearDepth, bool clearColor, ref Color backgroundColor, float depth);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGlobalVector_Injected(int nameID, ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGlobalColor_Injected(int nameID, ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGlobalMatrix_Injected(int nameID, ref Matrix4x4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetViewMatrix_Injected(ref Matrix4x4 view);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetProjectionMatrix_Injected(ref Matrix4x4 proj);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetViewProjectionMatrices_Injected(ref Matrix4x4 view, ref Matrix4x4 proj);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void IncrementUpdateCount_Injected(ref RenderTargetIdentifier dest);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetRenderTargetSingle_Internal_Injected(ref RenderTargetIdentifier rt, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetRenderTargetColorDepth_Internal_Injected(ref RenderTargetIdentifier color, ref RenderTargetIdentifier depth, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetRenderTargetMulti_Internal_Injected(RenderTargetIdentifier[] colors, ref RenderTargetIdentifier depth, RenderBufferLoadAction[] colorLoadActions, RenderBufferStoreAction[] colorStoreActions, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetRenderTargetColorDepthSubtarget_Injected(ref RenderTargetIdentifier color, ref RenderTargetIdentifier depth, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, int mipLevel, CubemapFace cubemapFace, int depthSlice);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetRenderTargetMultiSubtarget_Injected(RenderTargetIdentifier[] colors, ref RenderTargetIdentifier depth, RenderBufferLoadAction[] colorLoadActions, RenderBufferStoreAction[] colorStoreActions, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction, int mipLevel, CubemapFace cubemapFace, int depthSlice);
}
