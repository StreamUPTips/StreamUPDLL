namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void OpenInputPropertiesDialog(string sourceName, int obsConnection)
        {
            _CPH.ObsSendRaw("OpenInputPropertiesDialog", "{\"inputName\":\""+sourceName+"\"}", obsConnection);
        }

        public void OpenInputInteractDialog(string sourceName, int obsConnection)
        {
            _CPH.ObsSendRaw("OpenInputInteractDialog", "{\"inputName\":\""+sourceName+"\"}", obsConnection);
        }
    }
}
