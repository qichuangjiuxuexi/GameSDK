using AppBase.UI;


public partial class DialogueUIView
{

    public UIBinding Speaker => FindUIBind<UIBinding>("UI/Bg/Speaker");
    public UIBinding Desc => FindUIBind<UIBinding>("UI/Bg/Desc");
    public UIBinding Button => FindUIBind<UIBinding>("UI/Bg/Button");

}
