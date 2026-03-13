using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmostEngine.Screenshot;
//using UnityEngine.iOS;
using System.IO;
using System;

public class PhotoCaptureController : MonoBehaviour
{
	public ScreenshotManager screenManager;
	public GameObject[] objectsToHide;
	public GameObject[] objectsToShow;

	public bool autoShareAfterScreenshot;

	private string _sectionID;

	private DateTime _userDateTime;

    private void Awake()
    {
		for (int i = 0; i < objectsToShow.Length; ++i)
		{
			objectsToShow[i].SetActive(false);
		}
	}

    public void CaptureSingle()
	{
		_userDateTime = DateTime.Now;
		string formattedDateTime = (_userDateTime.ToString()).Replace('/', '.').Replace(':', '_').Trim();
		_sectionID = "photo_" + formattedDateTime;

		screenManager.m_Config.m_ShotMode = ScreenshotConfig.ShotMode.ONE_SHOT;
		screenManager.m_Config.m_FileName = Application.productName + "/" + _sectionID;

		PhotoPath.currentPhotoPath = screenManager.m_Config.GetPath() + screenManager.m_Config.m_FileName + ".png";

		for (int i = 0; i < objectsToHide.Length; ++i)
		{
			objectsToHide[i].SetActive(false);
		}
		for (int i = 0; i < objectsToShow.Length; ++i)
		{
			objectsToShow[i].SetActive(true);
		}

		ScreenshotManager.onCaptureEndDelegate += OnEndCaputePhoto;
		screenManager.Capture();
	}

	public void Share()
	{
		string path = PhotoPath.currentPhotoPath;

		byte[] bytes = File.ReadAllBytes(path);
		Texture2D image = new Texture2D(1, 1);
		image.LoadImage(bytes);

		ShareUtils.ShareImage(image, Application.productName);
	}

	private void OnEndCaputePhoto()
	{
		for (int i = 0; i < objectsToHide.Length; ++i)
		{
			objectsToHide[i].SetActive(true);
		}

		ScreenshotManager.onCaptureEndDelegate -= OnEndCaputePhoto;

		for (int i = 0; i < objectsToShow.Length; ++i)
		{
			objectsToShow[i].SetActive(false);
		}

		if(autoShareAfterScreenshot)
        {
			Share();
        }
	}
}