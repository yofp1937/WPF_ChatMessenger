using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Shared.Enums
{
    /// <summary>
    /// 메세지의 타입을 결정하는 enum
    /// </summary>
    public enum ChatMessageType
    {
        Text = 0,       // 일반 메세지
        System = 1,     // 입장, 퇴장 등 시스템 메세지
        // 추후 이모티콘, 이미지 등 확장 가능
    }
}
