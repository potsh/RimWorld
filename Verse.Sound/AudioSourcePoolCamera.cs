using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class AudioSourcePoolCamera
	{
		public GameObject cameraSourcesContainer;

		private List<AudioSource> sourcesCamera = new List<AudioSource>();

		private const int NumSourcesCamera = 16;

		public AudioSourcePoolCamera()
		{
			cameraSourcesContainer = new GameObject("OneShotSourcesCameraContainer");
			cameraSourcesContainer.transform.parent = Find.Camera.transform;
			cameraSourcesContainer.transform.localPosition = Vector3.zero;
			for (int i = 0; i < 16; i++)
			{
				AudioSource audioSource = AudioSourceMaker.NewAudioSourceOn(new GameObject("OneShotSourceCamera_" + i.ToString())
				{
					transform = 
					{
						parent = cameraSourcesContainer.transform,
						localPosition = Vector3.zero
					}
				});
				audioSource.bypassReverbZones = true;
				sourcesCamera.Add(audioSource);
			}
		}

		public AudioSource GetSourceCamera()
		{
			for (int i = 0; i < sourcesCamera.Count; i++)
			{
				AudioSource audioSource = sourcesCamera[i];
				if (!audioSource.isPlaying)
				{
					audioSource.clip = null;
					SoundFilterUtility.DisableAllFiltersOn(audioSource);
					return audioSource;
				}
			}
			return null;
		}
	}
}
