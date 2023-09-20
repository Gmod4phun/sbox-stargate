using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[Library]
public class MouseDragPanel : Panel
{
	public Panel Window => Parent;
	protected bool IsMoving { get; set; }
	protected Vector2 InitialPos { get; set; }

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		IsMoving = true;
		Style.Dirty();
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		IsMoving = false;
	}
}

public class Titlebar : MouseDragPanel
{
	protected Button CloseButton = null;
	protected Button Title = null;

	public Titlebar()
	{
		StyleSheet.Load( "sbox_stargate/ui/elements/titlebar/titlebar.scss" );
	}

	public void SetCloseButton( bool value, string text = "", Action onClicked = null )
	{
		if ( value && this.CloseButton == null )
		{
			CloseButton = this.Add.Button( text != "" ? text : "Close", "close", onClicked );
			return;
		}
		if ( value && this.CloseButton != null )
		{
			CloseButton.Delete( true );
			CloseButton = this.Add.Button( text != "" ? text : "Close", "close", onClicked );
			return;
		}
		if ( !value && this.CloseButton != null )
		{
			CloseButton.Delete( true );
			CloseButton = null;
			return;
		}
	}

	public void SetTitle( bool value, string title, Action onClicked = null )
	{
		if ( value )
		{
			if ( this.Title != null )
				this.Title.Delete( true );

			Title = this.Add.Button( title, "title", onClicked );
		}
		else
		{
			if ( this.Title != null )
				this.Title.Delete( true );
		}
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		InitialPos = (Mouse.Position - new Vector2( Window.Box.Left, Window.Box.Top )) * ScaleFromScreen;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( IsMoving )
		{
			var newPos = (Mouse.Position * ScaleFromScreen) - InitialPos;

			Window.Style.Left = Length.Pixels( newPos.x );
			Window.Style.Top = Length.Pixels( newPos.y );

			Window.Style.Dirty();
		}
	}
}
