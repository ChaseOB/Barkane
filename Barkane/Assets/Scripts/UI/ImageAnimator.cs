using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//from https://forum.unity.com/threads/trying-to-animate-ui-image.778355/
public class ImageAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private Image image;
 
        [SerializeField] private float fps = 10;
 
        public void Play()
        {
            Stop();
            StartCoroutine(AnimSequence());
        }
 
        public void Stop()
        {
            StopAllCoroutines();
            ShowFrame(0);
        }
 
        IEnumerator AnimSequence()
        {
            var delay = new WaitForSeconds(1 / fps);
            int index = 0;
            while(true)
            {
                if (index >= sprites.Length) index = 0;
                ShowFrame(index);
                index++;
                yield return delay;
            }
        }
 
        void ShowFrame(int index)
        {
            image.sprite = sprites[index];
        }
    }
