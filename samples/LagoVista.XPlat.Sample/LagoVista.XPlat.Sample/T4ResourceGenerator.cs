/*3/10/2019 10:58:10*/
using System.Globalization;
using System.Reflection;

//Resources:SampleResources:Model1_CheckBox
namespace LagoVista.XPlat.Sample.Resources
{
	public class SampleResources
	{
        private static global::System.Resources.ResourceManager _resourceManager;
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static global::System.Resources.ResourceManager ResourceManager 
		{
            get 
			{
                if (object.ReferenceEquals(_resourceManager, null)) 
				{
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.XPlat.Sample.Resources.SampleResources", typeof(SampleResources).GetTypeInfo().Assembly);
                    _resourceManager = temp;
                }
                return _resourceManager;
            }
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static string GetResourceString(string key, params string[] tokens)
		{
			var culture = CultureInfo.CurrentCulture;;
            var str = ResourceManager.GetString(key, culture);

			for(int i = 0; i < tokens.Length; i += 2)
				str = str.Replace(tokens[i], tokens[i+1]);
										
            return str;
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
		/*
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static HtmlString GetResourceHtmlString(string key, params string[] tokens)
		{
			var str = GetResourceString(key, tokens);
							
			if(str.StartsWith("HTML:"))
				str = str.Substring(5);

			return new HtmlString(str);
        }*/
		
		public static string Model1_CheckBox { get { return GetResourceString("Model1_CheckBox"); } }
//Resources:SampleResources:Model1_ChildList

		public static string Model1_ChildList { get { return GetResourceString("Model1_ChildList"); } }
//Resources:SampleResources:Model1_Description

		public static string Model1_Description { get { return GetResourceString("Model1_Description"); } }
//Resources:SampleResources:Model1_DropDown

		public static string Model1_DropDown { get { return GetResourceString("Model1_DropDown"); } }
//Resources:SampleResources:Model1_DropDown_Help

		public static string Model1_DropDown_Help { get { return GetResourceString("Model1_DropDown_Help"); } }
//Resources:SampleResources:Model1_DropDown_Option1

		public static string Model1_DropDown_Option1 { get { return GetResourceString("Model1_DropDown_Option1"); } }
//Resources:SampleResources:Model1_DropDown_Option2

		public static string Model1_DropDown_Option2 { get { return GetResourceString("Model1_DropDown_Option2"); } }
//Resources:SampleResources:Model1_DropDown_Option3

		public static string Model1_DropDown_Option3 { get { return GetResourceString("Model1_DropDown_Option3"); } }
//Resources:SampleResources:Model1_DropDown_Select

		public static string Model1_DropDown_Select { get { return GetResourceString("Model1_DropDown_Select"); } }
//Resources:SampleResources:Model1_Help

		public static string Model1_Help { get { return GetResourceString("Model1_Help"); } }
//Resources:SampleResources:Model1_LargeText

		public static string Model1_LargeText { get { return GetResourceString("Model1_LargeText"); } }
//Resources:SampleResources:Model1_LargeText_WaterMark

		public static string Model1_LargeText_WaterMark { get { return GetResourceString("Model1_LargeText_WaterMark"); } }
//Resources:SampleResources:Model1_LinkButton_Label

		public static string Model1_LinkButton_Label { get { return GetResourceString("Model1_LinkButton_Label"); } }
//Resources:SampleResources:Model1_LinkButton_Link

		public static string Model1_LinkButton_Link { get { return GetResourceString("Model1_LinkButton_Link"); } }
//Resources:SampleResources:Model1_Password

		public static string Model1_Password { get { return GetResourceString("Model1_Password"); } }
//Resources:SampleResources:Model1_Password_WaterMark

		public static string Model1_Password_WaterMark { get { return GetResourceString("Model1_Password_WaterMark"); } }
//Resources:SampleResources:Model1_Secret

		public static string Model1_Secret { get { return GetResourceString("Model1_Secret"); } }
//Resources:SampleResources:Model1_TextField1

		public static string Model1_TextField1 { get { return GetResourceString("Model1_TextField1"); } }
//Resources:SampleResources:Model1_TextField1_WaterMark

		public static string Model1_TextField1_WaterMark { get { return GetResourceString("Model1_TextField1_WaterMark"); } }
//Resources:SampleResources:Model1_Title

		public static string Model1_Title { get { return GetResourceString("Model1_Title"); } }

		public static class Names
		{
			public const string Model1_CheckBox = "Model1_CheckBox";
			public const string Model1_ChildList = "Model1_ChildList";
			public const string Model1_Description = "Model1_Description";
			public const string Model1_DropDown = "Model1_DropDown";
			public const string Model1_DropDown_Help = "Model1_DropDown_Help";
			public const string Model1_DropDown_Option1 = "Model1_DropDown_Option1";
			public const string Model1_DropDown_Option2 = "Model1_DropDown_Option2";
			public const string Model1_DropDown_Option3 = "Model1_DropDown_Option3";
			public const string Model1_DropDown_Select = "Model1_DropDown_Select";
			public const string Model1_Help = "Model1_Help";
			public const string Model1_LargeText = "Model1_LargeText";
			public const string Model1_LargeText_WaterMark = "Model1_LargeText_WaterMark";
			public const string Model1_LinkButton_Label = "Model1_LinkButton_Label";
			public const string Model1_LinkButton_Link = "Model1_LinkButton_Link";
			public const string Model1_Password = "Model1_Password";
			public const string Model1_Password_WaterMark = "Model1_Password_WaterMark";
			public const string Model1_Secret = "Model1_Secret";
			public const string Model1_TextField1 = "Model1_TextField1";
			public const string Model1_TextField1_WaterMark = "Model1_TextField1_WaterMark";
			public const string Model1_Title = "Model1_Title";
		}
	}
}

