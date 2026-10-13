using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace FT8215Debugger;

public class CommonFunc
{
	public static void SetBinding2ComboBox(ComboBox sourceCtrl, ComboBox targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(Selector.SelectedItemProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(Selector.SelectedItemProperty, binding2);
	}

	public static void SetBinding2TextBox(TextBox sourceCtrl, TextBox targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(TextBox.TextProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(TextBox.TextProperty, binding2);
	}

	public static void SetBinding2TextBlock(TextBlock sourceCtrl, TextBlock targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(TextBlock.TextProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(TextBlock.TextProperty, binding2);
	}

	public static void SetBinding2TextBlock2(TextBlock sourceCtrl, TextBlock targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(TextBlock.BaselineOffsetProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(TextBlock.BaselineOffsetProperty, binding2);
	}

	public static void SetBinding2Slider(Slider sourceCtrl, Slider targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(RangeBase.ValueProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(RangeBase.ValueProperty, binding2);
	}

	public static void SetBinding2RadioBtn(RadioButton sourceCtrl, RadioButton targetCtrl)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(ToggleButton.IsCheckedProperty);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(ToggleButton.IsCheckedProperty, binding2);
	}

	public static void SetBinding2RadioBtn(RadioButton sourceCtrl, RadioButton targetCtrl, DependencyProperty property)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(property);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(property, binding2);
	}

	public static void SetBinding2Label(Label sourceCtrl, Label targetCtrl, DependencyProperty property)
	{
		Binding binding = new Binding();
		binding.Path = new PropertyPath(property);
		binding.Source = sourceCtrl;
		binding.Mode = BindingMode.TwoWay;
		Binding binding2 = binding;
		targetCtrl.SetBinding(property, binding2);
	}

	public static UIElement SerializeControl(UIElement sourceCtrl)
	{
		string s = XamlWriter.Save(sourceCtrl);
		StringReader input = new StringReader(s);
		XmlReader reader = XmlReader.Create(input);
		return (UIElement)XamlReader.Load(reader);
	}

	public static UIElement SerializeControl2(UIElement sourceCtrl)
	{
		string xamlText = XamlWriter.Save(sourceCtrl);
		return (UIElement)XamlReader.Parse(xamlText);
	}
}
