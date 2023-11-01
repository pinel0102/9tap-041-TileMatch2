using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

namespace I2.SmartEdge
{
	[Serializable]
	public class UnityEventSEAnimation : UnityEvent<SE_Animation> { }

	[Serializable]
	public class SE_AnimationSlot
	{
		public string _LocalSerializedData;
		public SE_AnimationPreset _Preset;

		//public event System.Action<SE_Animation> _OnFinished;

		public SE_Animation _Animation
		{
			get
			{
				if (mAnimation == null)
					CreateAnimation();
				return mAnimation;
			}
		}

		public SE_Animation CreateAnimation()
		{
			if (_Preset != null)
				mAnimation = _Preset.CreateAnimation();
			else
				mAnimation = SE_Animation.LoadFromSerializedData(_LocalSerializedData) ?? new SE_Animation();
			return mAnimation;
		}

		public void Play( SmartEdge se )
		{
			var anim = _Animation;
			if (anim != null)
				anim.Play(se);
		}

		public bool IsPlaying()
		{
			var anim = _Animation;
			return (anim != null && anim.IsPlaying);
		}

		public string GetName()
		{
			if (_Preset != null)
				return _Preset.name;
			var NameKey = "XMLSchema\" Name=\"";
			int idx = _LocalSerializedData.IndexOf(NameKey);
			if (idx>=0)
			{
				idx+=NameKey.Length;
				int idx2 = _LocalSerializedData.IndexOf("\"", idx+1);
				return _LocalSerializedData.Substring(idx, idx2 - idx);
			}
			return "Custom";
		}

		SE_Animation mAnimation;
	}

	public partial class SmartEdge
	{
		public enum eTimeSource { Game, Real };
		public eTimeSource _TimeSource = eTimeSource.Game;

		public int _OnEnable_PlayAnim = -1;
		public SE_AnimationSlot[] _AnimationSlots = new SE_AnimationSlot[0];

		public UnityEventSEAnimation _OnAnimation_Finished = new UnityEventSEAnimation();


		private float mLastRealtimeUpdate;
		[NonSerialized] public List<SE_Animation> mPlayingAnimations = new List<SE_Animation>();

		public bool UpdateAnimations()
		{
			//---- Compute Delta Time ------------------
			float dt = 0;
			if (_TimeSource==eTimeSource.Game)
				dt = Time.deltaTime;
			else
			{
				float currentRealtime = Time.realtimeSinceStartup;
				if (mLastRealtimeUpdate>0)
					dt = currentRealtime - mLastRealtimeUpdate;
				mLastRealtimeUpdate = currentRealtime;
			}
			//---- Update Animations ------------------
			bool makeMaterialDirty = false;
			bool makeVerticesDirty = false;

			for (int i=mPlayingAnimations.Count-1; i>=0; --i)
			{
				var anim = mPlayingAnimations[i];
				if (anim.IsPlaying)
					anim.AnimUpdate(dt, this, ref makeMaterialDirty, ref makeVerticesDirty);
			}

			if (makeMaterialDirty || makeVerticesDirty)
				MarkWidgetAsChanged( makeVerticesDirty, makeMaterialDirty );

			return mPlayingAnimations.Count>0;
		}

		void ApplyAnimations_Characters()
		{
			for (int i = 0; i<mPlayingAnimations.Count; ++i)
				mPlayingAnimations[i].Apply_Characters(this);
		}

		void ApplyAnimations_Vertices()
		{
			for (int i = 0; i < mPlayingAnimations.Count; ++i)
				mPlayingAnimations[i].Apply_Vertices(this);
		}


		public SE_AnimationSlot GetAnimationSlot( string slotName )
		{
			foreach (var slot in _AnimationSlots)
				if (slot.GetName() == slotName)
					return slot;
			return null;
		}

		public SE_AnimationSlot GetAnimationSlot( SE_AnimationPreset preset)
		{
			foreach (var slot in _AnimationSlots)
				if (slot._Preset == preset)
					return slot;
			return null;
		}

		public SE_Animation GetPlayingAnimation(string animName)
		{
			for (int i = 0; i < mPlayingAnimations.Count; ++i)
				if (mPlayingAnimations[i].Name == animName)
					return mPlayingAnimations[i];
			return null;
		}


		public void StopAllAnimations( bool ExecuteCallbacks = true )
		{
			for (int i = mPlayingAnimations.Count-1; i>=0 ; --i)
				mPlayingAnimations[i].Stop(this, ExecuteCallbacks);
			mPlayingAnimations.Clear();
			MarkWidgetAsChanged(true, true);
		}

		public SE_Animation GetPlayingAnimation()
		{
			return mPlayingAnimations.Count == 0 ? null : mPlayingAnimations[0];
		}

		public void StopAnimation(string animationName, bool ExecuteCallbacks = true)
		{
			for (int i = 0; i < mPlayingAnimations.Count; ++i)
				if (mPlayingAnimations[i].Name == animationName)
				{
					mPlayingAnimations[i].Stop(this, ExecuteCallbacks);
					return;
				}
		}

		public void StopAnimation(SE_AnimationPreset preset, bool ExecuteCallbacks=true)
		{
			if (preset == null)
				return;

			// Check if the preset is from the animation's slots
			var slot = GetAnimationSlot(preset);
			if (slot!=null)
			{
				var anim = slot._Animation;
				if (!mPlayingAnimations.Contains(anim))
					return;

				anim.Stop(this, ExecuteCallbacks);
				return;
			}

			// if not, stop animation by name
			StopAnimation(preset.name, ExecuteCallbacks);
		}

		public void PlayAnimation(string slotName)
		{
			PlayAnim(slotName);
		}
		public void PlayAnimation(SE_AnimationPreset preset)
		{
			PlayAnim(preset);
		}

		public SE_Animation PlayAnim(SE_AnimationPreset preset)
		{
			if (preset == null)
				return null;

			SE_Animation anim;

			var slot = GetAnimationSlot(preset);
			if (slot != null)
			{
				anim = slot._Animation;
				anim.Play(this);
				return anim;
			}

			anim = preset.CreateAnimation();
			anim.Play(this);
			return anim;
		}


		public SE_Animation PlayAnim(string slotName)
		{
			var slot = GetAnimationSlot(slotName);
			if (slot != null)
			{
				var anim = slot._Animation;
				anim.Play(this);
				return anim;
			}

			return null;
		}
	}
}
