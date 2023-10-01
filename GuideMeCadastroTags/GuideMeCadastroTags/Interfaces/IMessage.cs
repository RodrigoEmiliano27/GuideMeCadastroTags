using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMeCadastroTags.Interfaces
{
    public interface IMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
        void CustomAlert(string message, long time);
    }
}
