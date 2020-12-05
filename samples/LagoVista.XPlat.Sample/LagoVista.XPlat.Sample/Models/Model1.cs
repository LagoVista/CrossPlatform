using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample.Models
{
    public enum DropDown1Options
    {
        [EnumLabel(Model1.DropDown_Option1, Resources.SampleResources.Names.Model1_DropDown_Option1,typeof(Resources.SampleResources))]
        Option1,
        [EnumLabel(Model1.DropDown_Option2, Resources.SampleResources.Names.Model1_DropDown_Option2, typeof(Resources.SampleResources))]
        Option2,
        [EnumLabel(Model1.DropDown_Option3, Resources.SampleResources.Names.Model1_DropDown_Option3, typeof(Resources.SampleResources))]
        Option3
    }

    [EntityDescription("Sample Data", Resources.SampleResources.Names.Model1_Title, Resources.SampleResources.Names.Model1_Help, 
        Resources.SampleResources.Names.Model1_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(Resources.SampleResources))]
    public class Model1
    {
        public const string DropDown_Option1 = "option1";
        public const string DropDown_Option2 = "option2";
        public const string DropDown_Option3 = "option3";

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_Secret, 
            WaterMark: Resources.SampleResources.Names.Model1_TextField1_WaterMark,
            FieldType: FieldTypes.Secret, ResourceType: typeof(Resources.SampleResources))]
        public string MySecretField { get; set; }
    
        [FormField(LabelResource:Resources.SampleResources.Names.Model1_TextField1,
            WaterMark: Resources.SampleResources.Names.Model1_TextField1_WaterMark,
            FieldType: FieldTypes.Text, ResourceType:typeof(Resources.SampleResources))]
        public string TextField1 { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_DropDown, 
            FieldType: FieldTypes.Picker, EnumType:typeof(DropDown1Options), 
            WaterMark:Resources.SampleResources.Names.Model1_DropDown_Select, ResourceType:
            typeof(Resources.SampleResources))]
        public EntityHeader DropDownBox1 { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_CheckBox, 
            FieldType: FieldTypes.CheckBox, ResourceType:typeof(Resources.SampleResources))]
        public bool CheckBox1 { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_LargeText,
            WaterMark: Resources.SampleResources.Names.Model1_LargeText_WaterMark, 
            FieldType: FieldTypes.MultiLineText, ResourceType: typeof(Resources.SampleResources))]
        public string MultiLine1 { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_Password,
            WaterMark: Resources.SampleResources.Names.Model1_Password_WaterMark, 
            FieldType: FieldTypes.Password, ResourceType: typeof(Resources.SampleResources))]
        public string Password { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_ChildList, FieldType: 
            FieldTypes.ChildList, ResourceType: typeof(Resources.SampleResources))]
        public List<Model2> Model2Litems { get; set; }

        [FormField(LabelResource: Resources.SampleResources.Names.Model1_LinkButton_Label,
            WaterMark: Resources.SampleResources.Names.Model1_LinkButton_Link,
            FieldType: FieldTypes.LinkButton, ResourceType: typeof(Resources.SampleResources))]
        public string LinkButton { get; set; }
    }
}
