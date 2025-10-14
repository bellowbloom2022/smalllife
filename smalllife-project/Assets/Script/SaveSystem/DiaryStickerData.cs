using System;
using System.Collections.Generic;

[System.Serializable]
public class DiaryStickerEntry
{
    public string goalKey;      // 唯一：如 "0_2"
    public int slotIndex;       // -1 = 未贴 | 0/1/2... = Slot 索引
    public bool animationPlayed; // 用于贴纸出现时一次性动画

    public DiaryStickerEntry(string key, int index = -1)
    {
        goalKey = key;
        slotIndex = index;
        animationPlayed = false;
    }
}
