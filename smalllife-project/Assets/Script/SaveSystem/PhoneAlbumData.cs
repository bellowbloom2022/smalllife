using System;
using System.Collections.Generic;

[Serializable]
public class PhoneAlbumData
{
    // 已解锁的照片ID列表，例如 "Spot01_1", "Spot02_3"
    public List<string> unlockedPhotos = new List<string>();

    // 可选：记录是否已查看，用于红点逻辑
    public HashSet<string> viewedPhotos = new HashSet<string>();
    public bool HasViewed(string goalKey) => viewedPhotos.Contains(goalKey);
    public void MarkAsViewed(string goalKey)
    {
        if (!viewedPhotos.Contains(goalKey))
            viewedPhotos.Add(goalKey);
    }

    public bool IsPhotoUnlocked(string photoID)
    {
        return unlockedPhotos.Contains(photoID);
    }

    public void UnlockPhoto(string photoID)
    {
        if (!unlockedPhotos.Contains(photoID))
            unlockedPhotos.Add(photoID);
    }

    public bool IsPhotoViewed(string photoID)
    {
        return viewedPhotos.Contains(photoID);
    }

    public void MarkPhotoViewed(string photoID)
    {
        viewedPhotos.Add(photoID);
    }
}
