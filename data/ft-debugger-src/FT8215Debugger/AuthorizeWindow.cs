using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace FT8215Debugger;

public partial class AuthorizeWindow : Window, IComponentConnector
{
	public AuthorizeWindow()
	{
		InitializeComponent();
		FocusManager.SetFocusedElement(this, passwordBox_Super);
		passwordBox_Super.SelectAll();
		passwordBox_Super.Focus();
	}

	private bool CheckPassword()
	{
		passwordBox_Super.Password = passwordBox_Super.Password.Trim();
		string password = passwordBox_Super.Password;
		if (password == "fT8202")
		{
			return true;
		}
		MessageBox.Show("Error input!", "Message", MessageBoxButton.OK);
		return false;
	}

	private void btn_OK_Click(object sender, RoutedEventArgs e)
	{
		if (CheckPassword())
		{
			base.DialogResult = true;
			Close();
		}
	}

	private void btn_Cancel_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}

	private void passwordBox_Super_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && CheckPassword())
		{
			base.DialogResult = true;
			Close();
		}
	}
}
