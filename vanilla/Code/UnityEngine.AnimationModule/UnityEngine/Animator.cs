using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Playables;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("Modules/Animation/Animator.h")]
[NativeHeader("Modules/Animation/ScriptBindings/Animator.bindings.h")]
[NativeHeader("Modules/Animation/ScriptBindings/AnimatorControllerParameter.bindings.h")]
[UsedByNativeCode]
public class Animator : Behaviour
{
	public extern bool isOptimizable
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsOptimizable")]
		get;
	}

	public extern bool isHuman
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsHuman")]
		get;
	}

	public extern bool hasRootMotion
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("HasRootMotion")]
		get;
	}

	internal extern bool isRootPositionOrRotationControlledByCurves
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsRootTranslationOrRotationControllerByCurves")]
		get;
	}

	public extern float humanScale
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool isInitialized
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsInitialized")]
		get;
	}

	public Vector3 deltaPosition
	{
		get
		{
			get_deltaPosition_Injected(out var ret);
			return ret;
		}
	}

	public Quaternion deltaRotation
	{
		get
		{
			get_deltaRotation_Injected(out var ret);
			return ret;
		}
	}

	public Vector3 velocity
	{
		get
		{
			get_velocity_Injected(out var ret);
			return ret;
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			get_angularVelocity_Injected(out var ret);
			return ret;
		}
	}

	public Vector3 rootPosition
	{
		[NativeMethod("GetAvatarPosition")]
		get
		{
			get_rootPosition_Injected(out var ret);
			return ret;
		}
		[NativeMethod("SetAvatarPosition")]
		set
		{
			set_rootPosition_Injected(ref value);
		}
	}

	public Quaternion rootRotation
	{
		[NativeMethod("GetAvatarRotation")]
		get
		{
			get_rootRotation_Injected(out var ret);
			return ret;
		}
		[NativeMethod("SetAvatarRotation")]
		set
		{
			set_rootRotation_Injected(ref value);
		}
	}

	public extern bool applyRootMotion
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[Obsolete("Animator.linearVelocityBlending is no longer used and has been deprecated.")]
	public extern bool linearVelocityBlending
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[Obsolete("Animator.animatePhysics has been deprecated. Use Animator.updateMode instead.")]
	public bool animatePhysics
	{
		get
		{
			return updateMode == AnimatorUpdateMode.AnimatePhysics;
		}
		set
		{
			updateMode = (value ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal);
		}
	}

	public extern AnimatorUpdateMode updateMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool hasTransformHierarchy
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	internal extern bool allowConstantClipSamplingOptimization
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float gravityWeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public Vector3 bodyPosition
	{
		get
		{
			CheckIfInIKPass();
			return bodyPositionInternal;
		}
		set
		{
			CheckIfInIKPass();
			bodyPositionInternal = value;
		}
	}

	internal Vector3 bodyPositionInternal
	{
		[NativeMethod("GetBodyPosition")]
		get
		{
			get_bodyPositionInternal_Injected(out var ret);
			return ret;
		}
		[NativeMethod("SetBodyPosition")]
		set
		{
			set_bodyPositionInternal_Injected(ref value);
		}
	}

	public Quaternion bodyRotation
	{
		get
		{
			CheckIfInIKPass();
			return bodyRotationInternal;
		}
		set
		{
			CheckIfInIKPass();
			bodyRotationInternal = value;
		}
	}

	internal Quaternion bodyRotationInternal
	{
		[NativeMethod("GetBodyRotation")]
		get
		{
			get_bodyRotationInternal_Injected(out var ret);
			return ret;
		}
		[NativeMethod("SetBodyRotation")]
		set
		{
			set_bodyRotationInternal_Injected(ref value);
		}
	}

	public extern bool stabilizeFeet
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int layerCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern AnimatorControllerParameter[] parameters
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction(Name = "AnimatorBindings::GetParameters", HasExplicitThis = true)]
		get;
	}

	public extern int parameterCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern float feetPivotActive
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float pivotWeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public Vector3 pivotPosition
	{
		get
		{
			get_pivotPosition_Injected(out var ret);
			return ret;
		}
	}

	public extern bool isMatchingTarget
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("IsMatchingTarget")]
		get;
	}

	public extern float speed
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector3 targetPosition
	{
		get
		{
			get_targetPosition_Injected(out var ret);
			return ret;
		}
	}

	public Quaternion targetRotation
	{
		get
		{
			get_targetRotation_Injected(out var ret);
			return ret;
		}
	}

	internal extern Transform avatarRoot
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern AnimatorCullingMode cullingMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float playbackTime
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public float recorderStartTime
	{
		get
		{
			return GetRecorderStartTime();
		}
		set
		{
		}
	}

	public float recorderStopTime
	{
		get
		{
			return GetRecorderStopTime();
		}
		set
		{
		}
	}

	public extern AnimatorRecorderMode recorderMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern RuntimeAnimatorController runtimeAnimatorController
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool hasBoundPlayables
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("HasBoundPlayables")]
		get;
	}

	public extern Avatar avatar
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public PlayableGraph playableGraph
	{
		get
		{
			PlayableGraph graph = default(PlayableGraph);
			GetCurrentGraph(ref graph);
			return graph;
		}
	}

	public extern bool layersAffectMassCenter
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float leftFeetBottomHeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern float rightFeetBottomHeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeConditional("UNITY_EDITOR")]
	internal extern bool supportsOnAnimatorMove
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("SupportsOnAnimatorMove")]
		get;
	}

	public extern bool logWarnings
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool fireEvents
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool keepAnimatorControllerStateOnDisable
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public float GetFloat(string name)
	{
		return GetFloatString(name);
	}

	public float GetFloat(int id)
	{
		return GetFloatID(id);
	}

	public void SetFloat(string name, float value)
	{
		SetFloatString(name, value);
	}

	public void SetFloat(string name, float value, float dampTime, float deltaTime)
	{
		SetFloatStringDamp(name, value, dampTime, deltaTime);
	}

	public void SetFloat(int id, float value)
	{
		SetFloatID(id, value);
	}

	public void SetFloat(int id, float value, float dampTime, float deltaTime)
	{
		SetFloatIDDamp(id, value, dampTime, deltaTime);
	}

	public bool GetBool(string name)
	{
		return GetBoolString(name);
	}

	public bool GetBool(int id)
	{
		return GetBoolID(id);
	}

	public void SetBool(string name, bool value)
	{
		SetBoolString(name, value);
	}

	public void SetBool(int id, bool value)
	{
		SetBoolID(id, value);
	}

	public int GetInteger(string name)
	{
		return GetIntegerString(name);
	}

	public int GetInteger(int id)
	{
		return GetIntegerID(id);
	}

	public void SetInteger(string name, int value)
	{
		SetIntegerString(name, value);
	}

	public void SetInteger(int id, int value)
	{
		SetIntegerID(id, value);
	}

	public void SetTrigger(string name)
	{
		SetTriggerString(name);
	}

	public void SetTrigger(int id)
	{
		SetTriggerID(id);
	}

	public void ResetTrigger(string name)
	{
		ResetTriggerString(name);
	}

	public void ResetTrigger(int id)
	{
		ResetTriggerID(id);
	}

	public bool IsParameterControlledByCurve(string name)
	{
		return IsParameterControlledByCurveString(name);
	}

	public bool IsParameterControlledByCurve(int id)
	{
		return IsParameterControlledByCurveID(id);
	}

	public Vector3 GetIKPosition(AvatarIKGoal goal)
	{
		CheckIfInIKPass();
		return GetGoalPosition(goal);
	}

	private Vector3 GetGoalPosition(AvatarIKGoal goal)
	{
		GetGoalPosition_Injected(goal, out var ret);
		return ret;
	}

	public void SetIKPosition(AvatarIKGoal goal, Vector3 goalPosition)
	{
		CheckIfInIKPass();
		SetGoalPosition(goal, goalPosition);
	}

	private void SetGoalPosition(AvatarIKGoal goal, Vector3 goalPosition)
	{
		SetGoalPosition_Injected(goal, ref goalPosition);
	}

	public Quaternion GetIKRotation(AvatarIKGoal goal)
	{
		CheckIfInIKPass();
		return GetGoalRotation(goal);
	}

	private Quaternion GetGoalRotation(AvatarIKGoal goal)
	{
		GetGoalRotation_Injected(goal, out var ret);
		return ret;
	}

	public void SetIKRotation(AvatarIKGoal goal, Quaternion goalRotation)
	{
		CheckIfInIKPass();
		SetGoalRotation(goal, goalRotation);
	}

	private void SetGoalRotation(AvatarIKGoal goal, Quaternion goalRotation)
	{
		SetGoalRotation_Injected(goal, ref goalRotation);
	}

	public float GetIKPositionWeight(AvatarIKGoal goal)
	{
		CheckIfInIKPass();
		return GetGoalWeightPosition(goal);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float GetGoalWeightPosition(AvatarIKGoal goal);

	public void SetIKPositionWeight(AvatarIKGoal goal, float value)
	{
		CheckIfInIKPass();
		SetGoalWeightPosition(goal, value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGoalWeightPosition(AvatarIKGoal goal, float value);

	public float GetIKRotationWeight(AvatarIKGoal goal)
	{
		CheckIfInIKPass();
		return GetGoalWeightRotation(goal);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float GetGoalWeightRotation(AvatarIKGoal goal);

	public void SetIKRotationWeight(AvatarIKGoal goal, float value)
	{
		CheckIfInIKPass();
		SetGoalWeightRotation(goal, value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGoalWeightRotation(AvatarIKGoal goal, float value);

	public Vector3 GetIKHintPosition(AvatarIKHint hint)
	{
		CheckIfInIKPass();
		return GetHintPosition(hint);
	}

	private Vector3 GetHintPosition(AvatarIKHint hint)
	{
		GetHintPosition_Injected(hint, out var ret);
		return ret;
	}

	public void SetIKHintPosition(AvatarIKHint hint, Vector3 hintPosition)
	{
		CheckIfInIKPass();
		SetHintPosition(hint, hintPosition);
	}

	private void SetHintPosition(AvatarIKHint hint, Vector3 hintPosition)
	{
		SetHintPosition_Injected(hint, ref hintPosition);
	}

	public float GetIKHintPositionWeight(AvatarIKHint hint)
	{
		CheckIfInIKPass();
		return GetHintWeightPosition(hint);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float GetHintWeightPosition(AvatarIKHint hint);

	public void SetIKHintPositionWeight(AvatarIKHint hint, float value)
	{
		CheckIfInIKPass();
		SetHintWeightPosition(hint, value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetHintWeightPosition(AvatarIKHint hint, float value);

	public void SetLookAtPosition(Vector3 lookAtPosition)
	{
		CheckIfInIKPass();
		SetLookAtPositionInternal(lookAtPosition);
	}

	[NativeMethod("SetLookAtPosition")]
	private void SetLookAtPositionInternal(Vector3 lookAtPosition)
	{
		SetLookAtPositionInternal_Injected(ref lookAtPosition);
	}

	public void SetLookAtWeight(float weight)
	{
		CheckIfInIKPass();
		SetLookAtWeightInternal(weight, 0f, 1f, 0f, 0.5f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight)
	{
		CheckIfInIKPass();
		SetLookAtWeightInternal(weight, bodyWeight, 1f, 0f, 0.5f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
	{
		CheckIfInIKPass();
		SetLookAtWeightInternal(weight, bodyWeight, headWeight, 0f, 0.5f);
	}

	public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight)
	{
		CheckIfInIKPass();
		SetLookAtWeightInternal(weight, bodyWeight, headWeight, eyesWeight, 0.5f);
	}

	public void SetLookAtWeight(float weight, [UnityEngine.Internal.DefaultValue("0.0f")] float bodyWeight, [UnityEngine.Internal.DefaultValue("1.0f")] float headWeight, [UnityEngine.Internal.DefaultValue("0.0f")] float eyesWeight, [UnityEngine.Internal.DefaultValue("0.5f")] float clampWeight)
	{
		CheckIfInIKPass();
		SetLookAtWeightInternal(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("SetLookAtWeight")]
	private extern void SetLookAtWeightInternal(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight);

	public void SetBoneLocalRotation(HumanBodyBones humanBoneId, Quaternion rotation)
	{
		CheckIfInIKPass();
		SetBoneLocalRotationInternal(HumanTrait.GetBoneIndexFromMono((int)humanBoneId), rotation);
	}

	[NativeMethod("SetBoneLocalRotation")]
	private void SetBoneLocalRotationInternal(int humanBoneId, Quaternion rotation)
	{
		SetBoneLocalRotationInternal_Injected(humanBoneId, ref rotation);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern ScriptableObject GetBehaviour([NotNull] Type type);

	public T GetBehaviour<T>() where T : StateMachineBehaviour
	{
		return GetBehaviour(typeof(T)) as T;
	}

	private static T[] ConvertStateMachineBehaviour<T>(ScriptableObject[] rawObjects) where T : StateMachineBehaviour
	{
		if (rawObjects == null)
		{
			return null;
		}
		T[] array = new T[rawObjects.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (T)rawObjects[i];
		}
		return array;
	}

	public T[] GetBehaviours<T>() where T : StateMachineBehaviour
	{
		return ConvertStateMachineBehaviour<T>(InternalGetBehaviours(typeof(T)));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::InternalGetBehaviours", HasExplicitThis = true)]
	internal extern ScriptableObject[] InternalGetBehaviours([NotNull] Type type);

	public StateMachineBehaviour[] GetBehaviours(int fullPathHash, int layerIndex)
	{
		return InternalGetBehavioursByKey(fullPathHash, layerIndex, typeof(StateMachineBehaviour)) as StateMachineBehaviour[];
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::InternalGetBehavioursByKey", HasExplicitThis = true)]
	internal extern ScriptableObject[] InternalGetBehavioursByKey(int fullPathHash, int layerIndex, [NotNull] Type type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern string GetLayerName(int layerIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetLayerIndex(string layerName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern float GetLayerWeight(int layerIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetLayerWeight(int layerIndex, float weight);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetAnimatorStateInfo(int layerIndex, StateInfoIndex stateInfoIndex, out AnimatorStateInfo info);

	public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
	{
		GetAnimatorStateInfo(layerIndex, StateInfoIndex.CurrentState, out var info);
		return info;
	}

	public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex)
	{
		GetAnimatorStateInfo(layerIndex, StateInfoIndex.NextState, out var info);
		return info;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetAnimatorTransitionInfo(int layerIndex, out AnimatorTransitionInfo info);

	public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex)
	{
		GetAnimatorTransitionInfo(layerIndex, out var info);
		return info;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern int GetAnimatorClipInfoCount(int layerIndex, bool current);

	public int GetCurrentAnimatorClipInfoCount(int layerIndex)
	{
		return GetAnimatorClipInfoCount(layerIndex, current: true);
	}

	public int GetNextAnimatorClipInfoCount(int layerIndex)
	{
		return GetAnimatorClipInfoCount(layerIndex, current: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetCurrentAnimatorClipInfo", HasExplicitThis = true)]
	public extern AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetNextAnimatorClipInfo", HasExplicitThis = true)]
	public extern AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex);

	public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
	{
		if (clips == null)
		{
			throw new ArgumentNullException("clips");
		}
		GetAnimatorClipInfoInternal(layerIndex, isCurrent: true, clips);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetAnimatorClipInfoInternal", HasExplicitThis = true)]
	private extern void GetAnimatorClipInfoInternal(int layerIndex, bool isCurrent, object clips);

	public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
	{
		if (clips == null)
		{
			throw new ArgumentNullException("clips");
		}
		GetAnimatorClipInfoInternal(layerIndex, isCurrent: false, clips);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool IsInTransition(int layerIndex);

	public AnimatorControllerParameter GetParameter(int index)
	{
		AnimatorControllerParameter[] array = parameters;
		if (index < 0 || index >= parameters.Length)
		{
			throw new IndexOutOfRangeException("Index must be between 0 and " + parameters.Length);
		}
		return array[index];
	}

	private void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, int targetBodyPart, MatchTargetWeightMask weightMask, float startNormalizedTime, float targetNormalizedTime, bool completeMatch)
	{
		MatchTarget_Injected(ref matchPosition, ref matchRotation, targetBodyPart, ref weightMask, startNormalizedTime, targetNormalizedTime, completeMatch);
	}

	public void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget targetBodyPart, MatchTargetWeightMask weightMask, float startNormalizedTime)
	{
		MatchTarget(matchPosition, matchRotation, (int)targetBodyPart, weightMask, startNormalizedTime, 1f, completeMatch: true);
	}

	public void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget targetBodyPart, MatchTargetWeightMask weightMask, float startNormalizedTime, [UnityEngine.Internal.DefaultValue("1")] float targetNormalizedTime)
	{
		MatchTarget(matchPosition, matchRotation, (int)targetBodyPart, weightMask, startNormalizedTime, targetNormalizedTime, completeMatch: true);
	}

	public void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget targetBodyPart, MatchTargetWeightMask weightMask, float startNormalizedTime, [UnityEngine.Internal.DefaultValue("1")] float targetNormalizedTime, [UnityEngine.Internal.DefaultValue("true")] bool completeMatch)
	{
		MatchTarget(matchPosition, matchRotation, (int)targetBodyPart, weightMask, startNormalizedTime, targetNormalizedTime, completeMatch);
	}

	public void InterruptMatchTarget()
	{
		InterruptMatchTarget(completeMatch: true);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void InterruptMatchTarget([UnityEngine.Internal.DefaultValue("true")] bool completeMatch);

	[Obsolete("ForceStateNormalizedTime is deprecated. Please use Play or CrossFade instead.")]
	public void ForceStateNormalizedTime(float normalizedTime)
	{
		Play(0, 0, normalizedTime);
	}

	public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration)
	{
		float normalizedTransitionTime = 0f;
		float fixedTimeOffset = 0f;
		int layer = -1;
		CrossFadeInFixedTime(StringToHash(stateName), fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int layer)
	{
		float normalizedTransitionTime = 0f;
		float fixedTimeOffset = 0f;
		CrossFadeInFixedTime(StringToHash(stateName), fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int layer, float fixedTimeOffset)
	{
		float normalizedTransitionTime = 0f;
		CrossFadeInFixedTime(StringToHash(stateName), fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("0.0f")] float fixedTimeOffset, [UnityEngine.Internal.DefaultValue("0.0f")] float normalizedTransitionTime)
	{
		CrossFadeInFixedTime(StringToHash(stateName), fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(int stateHashName, float fixedTransitionDuration, int layer, float fixedTimeOffset)
	{
		float normalizedTransitionTime = 0f;
		CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(int stateHashName, float fixedTransitionDuration, int layer)
	{
		float normalizedTransitionTime = 0f;
		float fixedTimeOffset = 0f;
		CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFadeInFixedTime(int stateHashName, float fixedTransitionDuration)
	{
		float normalizedTransitionTime = 0f;
		float fixedTimeOffset = 0f;
		int layer = -1;
		CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::CrossFadeInFixedTime", HasExplicitThis = true)]
	public extern void CrossFadeInFixedTime(int stateHashName, float fixedTransitionDuration, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("0.0f")] float fixedTimeOffset, [UnityEngine.Internal.DefaultValue("0.0f")] float normalizedTransitionTime);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::WriteDefaultValues", HasExplicitThis = true)]
	public extern void WriteDefaultValues();

	public void CrossFade(string stateName, float normalizedTransitionDuration, int layer, float normalizedTimeOffset)
	{
		float normalizedTransitionTime = 0f;
		CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFade(string stateName, float normalizedTransitionDuration, int layer)
	{
		float normalizedTransitionTime = 0f;
		float normalizedTimeOffset = float.NegativeInfinity;
		CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFade(string stateName, float normalizedTransitionDuration)
	{
		float normalizedTransitionTime = 0f;
		float normalizedTimeOffset = float.NegativeInfinity;
		int layer = -1;
		CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFade(string stateName, float normalizedTransitionDuration, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float normalizedTimeOffset, [UnityEngine.Internal.DefaultValue("0.0f")] float normalizedTransitionTime)
	{
		CrossFade(StringToHash(stateName), normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::CrossFade", HasExplicitThis = true)]
	public extern void CrossFade(int stateHashName, float normalizedTransitionDuration, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("0.0f")] float normalizedTimeOffset, [UnityEngine.Internal.DefaultValue("0.0f")] float normalizedTransitionTime);

	public void CrossFade(int stateHashName, float normalizedTransitionDuration, int layer, float normalizedTimeOffset)
	{
		float normalizedTransitionTime = 0f;
		CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFade(int stateHashName, float normalizedTransitionDuration, int layer)
	{
		float normalizedTransitionTime = 0f;
		float normalizedTimeOffset = float.NegativeInfinity;
		CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void CrossFade(int stateHashName, float normalizedTransitionDuration)
	{
		float normalizedTransitionTime = 0f;
		float normalizedTimeOffset = float.NegativeInfinity;
		int layer = -1;
		CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
	}

	public void PlayInFixedTime(string stateName, int layer)
	{
		float fixedTime = float.NegativeInfinity;
		PlayInFixedTime(stateName, layer, fixedTime);
	}

	public void PlayInFixedTime(string stateName)
	{
		float fixedTime = float.NegativeInfinity;
		int layer = -1;
		PlayInFixedTime(stateName, layer, fixedTime);
	}

	public void PlayInFixedTime(string stateName, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float fixedTime)
	{
		PlayInFixedTime(StringToHash(stateName), layer, fixedTime);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::PlayInFixedTime", HasExplicitThis = true)]
	public extern void PlayInFixedTime(int stateNameHash, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float fixedTime);

	public void PlayInFixedTime(int stateNameHash, int layer)
	{
		float fixedTime = float.NegativeInfinity;
		PlayInFixedTime(stateNameHash, layer, fixedTime);
	}

	public void PlayInFixedTime(int stateNameHash)
	{
		float fixedTime = float.NegativeInfinity;
		int layer = -1;
		PlayInFixedTime(stateNameHash, layer, fixedTime);
	}

	public void Play(string stateName, int layer)
	{
		float normalizedTime = float.NegativeInfinity;
		Play(stateName, layer, normalizedTime);
	}

	public void Play(string stateName)
	{
		float normalizedTime = float.NegativeInfinity;
		int layer = -1;
		Play(stateName, layer, normalizedTime);
	}

	public void Play(string stateName, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float normalizedTime)
	{
		Play(StringToHash(stateName), layer, normalizedTime);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::Play", HasExplicitThis = true)]
	public extern void Play(int stateNameHash, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float normalizedTime);

	public void Play(int stateNameHash, int layer)
	{
		float normalizedTime = float.NegativeInfinity;
		Play(stateNameHash, layer, normalizedTime);
	}

	public void Play(int stateNameHash)
	{
		float normalizedTime = float.NegativeInfinity;
		int layer = -1;
		Play(stateNameHash, layer, normalizedTime);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetTarget(AvatarTarget targetIndex, float targetNormalizedTime);

	[Obsolete("Use mask and layers to control subset of transfroms in a skeleton.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool IsControlled(Transform transform)
	{
		return false;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern bool IsBoneTransform(Transform transform);

	public Transform GetBoneTransform(HumanBodyBones humanBoneId)
	{
		if (humanBoneId < HumanBodyBones.Hips || humanBoneId >= HumanBodyBones.LastBone)
		{
			throw new IndexOutOfRangeException("humanBoneId must be between 0 and " + HumanBodyBones.LastBone);
		}
		return GetBoneTransformInternal(HumanTrait.GetBoneIndexFromMono((int)humanBoneId));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetBoneTransform")]
	internal extern Transform GetBoneTransformInternal(int humanBoneId);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void StartPlayback();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void StopPlayback();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void StartRecording(int frameCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void StopRecording();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float GetRecorderStartTime();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float GetRecorderStopTime();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void ClearInternalControllerPlayable();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool HasState(int layerIndex, int stateID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ScriptingStringToCRC32", IsThreadSafe = true)]
	public static extern int StringToHash(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern string GetStats();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetCurrentGraph", HasExplicitThis = true)]
	private extern void GetCurrentGraph(ref PlayableGraph graph);

	private void CheckIfInIKPass()
	{
		if (logWarnings && !IsInIKPass())
		{
			Debug.LogWarning("Setting and getting Body Position/Rotation, IK Goals, Lookat and BoneLocalRotation should only be done in OnAnimatorIK or OnStateIK");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool IsInIKPass();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetFloatString", HasExplicitThis = true)]
	private extern void SetFloatString(string name, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetFloatID", HasExplicitThis = true)]
	private extern void SetFloatID(int id, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetFloatString", HasExplicitThis = true)]
	private extern float GetFloatString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetFloatID", HasExplicitThis = true)]
	private extern float GetFloatID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetBoolString", HasExplicitThis = true)]
	private extern void SetBoolString(string name, bool value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetBoolID", HasExplicitThis = true)]
	private extern void SetBoolID(int id, bool value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetBoolString", HasExplicitThis = true)]
	private extern bool GetBoolString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetBoolID", HasExplicitThis = true)]
	private extern bool GetBoolID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetIntegerString", HasExplicitThis = true)]
	private extern void SetIntegerString(string name, int value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetIntegerID", HasExplicitThis = true)]
	private extern void SetIntegerID(int id, int value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetIntegerString", HasExplicitThis = true)]
	private extern int GetIntegerString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::GetIntegerID", HasExplicitThis = true)]
	private extern int GetIntegerID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetTriggerString", HasExplicitThis = true)]
	private extern void SetTriggerString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetTriggerID", HasExplicitThis = true)]
	private extern void SetTriggerID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::ResetTriggerString", HasExplicitThis = true)]
	private extern void ResetTriggerString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::ResetTriggerID", HasExplicitThis = true)]
	private extern void ResetTriggerID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::IsParameterControlledByCurveString", HasExplicitThis = true)]
	private extern bool IsParameterControlledByCurveString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::IsParameterControlledByCurveID", HasExplicitThis = true)]
	private extern bool IsParameterControlledByCurveID(int id);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetFloatStringDamp", HasExplicitThis = true)]
	private extern void SetFloatStringDamp(string name, float value, float dampTime, float deltaTime);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "AnimatorBindings::SetFloatIDDamp", HasExplicitThis = true)]
	private extern void SetFloatIDDamp(int id, float value, float dampTime, float deltaTime);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR")]
	internal extern void OnUpdateModeChanged();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR")]
	internal extern void OnCullingModeChanged();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR")]
	internal extern void WriteDefaultPose();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("UpdateWithDelta")]
	public extern void Update(float deltaTime);

	public void Rebind()
	{
		Rebind(writeDefaultValues: true);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Rebind(bool writeDefaultValues);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void ApplyBuiltinRootMotion();

	[NativeConditional("UNITY_EDITOR")]
	internal void EvaluateController()
	{
		EvaluateController(0f);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void EvaluateController(float deltaTime);

	[NativeConditional("UNITY_EDITOR")]
	internal string GetCurrentStateName(int layerIndex)
	{
		return GetAnimatorStateName(layerIndex, current: true);
	}

	[NativeConditional("UNITY_EDITOR")]
	internal string GetNextStateName(int layerIndex)
	{
		return GetAnimatorStateName(layerIndex, current: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR")]
	private extern string GetAnimatorStateName(int layerIndex, bool current);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern string ResolveHash(int hash);

	[Obsolete("GetVector is deprecated.")]
	public Vector3 GetVector(string name)
	{
		return Vector3.zero;
	}

	[Obsolete("GetVector is deprecated.")]
	public Vector3 GetVector(int id)
	{
		return Vector3.zero;
	}

	[Obsolete("SetVector is deprecated.")]
	public void SetVector(string name, Vector3 value)
	{
	}

	[Obsolete("SetVector is deprecated.")]
	public void SetVector(int id, Vector3 value)
	{
	}

	[Obsolete("GetQuaternion is deprecated.")]
	public Quaternion GetQuaternion(string name)
	{
		return Quaternion.identity;
	}

	[Obsolete("GetQuaternion is deprecated.")]
	public Quaternion GetQuaternion(int id)
	{
		return Quaternion.identity;
	}

	[Obsolete("SetQuaternion is deprecated.")]
	public void SetQuaternion(string name, Quaternion value)
	{
	}

	[Obsolete("SetQuaternion is deprecated.")]
	public void SetQuaternion(int id, Quaternion value)
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_deltaPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_deltaRotation_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_velocity_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_angularVelocity_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_rootPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_rootPosition_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_rootRotation_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_rootRotation_Injected(ref Quaternion value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_bodyPositionInternal_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_bodyPositionInternal_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_bodyRotationInternal_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_bodyRotationInternal_Injected(ref Quaternion value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetGoalPosition_Injected(AvatarIKGoal goal, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGoalPosition_Injected(AvatarIKGoal goal, ref Vector3 goalPosition);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetGoalRotation_Injected(AvatarIKGoal goal, out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGoalRotation_Injected(AvatarIKGoal goal, ref Quaternion goalRotation);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetHintPosition_Injected(AvatarIKHint hint, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetHintPosition_Injected(AvatarIKHint hint, ref Vector3 hintPosition);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetLookAtPositionInternal_Injected(ref Vector3 lookAtPosition);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetBoneLocalRotationInternal_Injected(int humanBoneId, ref Quaternion rotation);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_pivotPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void MatchTarget_Injected(ref Vector3 matchPosition, ref Quaternion matchRotation, int targetBodyPart, ref MatchTargetWeightMask weightMask, float startNormalizedTime, float targetNormalizedTime, bool completeMatch);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_targetPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_targetRotation_Injected(out Quaternion ret);
}
