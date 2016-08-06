using System;

public class Game
{
	public Game ()
	{
	}
	
	public enum MoveDir
	{
		None,
		UpDown,
		LeftRight
	}
	
	public static float ItemWidth;
	public static float ItemHeight;
	
	public static int NumRow = 8;
	public static int NumCol = 5;
	
	public static bool IsClose(float x, float y, float threshold)
	{
		if (Math.Abs(x - y) <= threshold)
		{
			return  true;
		}
		
		return false;
	}
}


