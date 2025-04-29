using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI
{
    public class UIBinding : MonoBehaviour
    {
        public string BindingName;
        
        
        public Dictionary<Type, Component> components;

        public RectTransform RectTransform => Get<RectTransform>();
        public Button Button => Get<Button>();
        public Image Image => Get<Image>();
        public Text Text => Get<Text>();
        public TextMeshProUGUI TextMeshProUGUI => Get<TextMeshProUGUI>();
        public TextMeshPro TextMeshPro => Get<TextMeshPro>();


        public T Get<T>() where T : Component
        {
            if (components != null && components.TryGetValue(typeof(T), out Component component) && component != null)
            {
                return (T)component;
            }

            component = GetComponent<T>();
            if (component == null) return null;

            components ??= new Dictionary<Type, Component>();
            components[typeof(T)] = component;
        
            return(T)component;

        }

    }   
}
