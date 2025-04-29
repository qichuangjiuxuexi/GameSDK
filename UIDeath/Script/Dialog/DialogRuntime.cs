using System;
using UnityEngine;

namespace AppBase.UI.Dialog
{
	public class DialogRuntime : MonoBehaviour
	{
		UIDialog dialog;
		private void Awake(){
			dialog = GetComponent<UIDialog>();
			if (dialog != null)
			{
				dialog.OnAwake();
				dialog.OnBindComponents();
			}
		}
		private void Start(){
			dialog = GetComponent<UIDialog>();
			if (dialog != null)
			{
				dialog.OnStart();
				dialog.PlayOpenAnim(() => {
					dialog.dialogData.openCallback?.Invoke(dialog);
				});
			}
		}
	}
}
