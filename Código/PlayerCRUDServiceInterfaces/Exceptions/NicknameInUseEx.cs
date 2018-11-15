using System;
using System.Runtime.Serialization;

namespace PlayerCRUDServiceInterfaces
{
    [Serializable]
    public class NicknameInUseEx : Exception
    {
        override public string Message { get; }
        public NicknameInUseEx()
        {
            Message = "Already exists player with same nickname. Try again.";
        }
    }
}