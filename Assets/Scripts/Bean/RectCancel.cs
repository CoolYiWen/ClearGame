using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消除检测算法
/// </summary>
public class RectCancel
{
	enum DetectDir//检测方向
	{
		Up,
		Down,
		Left,
		Right,
	}
	
	public RectCancel()
	{
	}
	
    /// <summary>
    /// 消除判断
    /// </summary>
	public ArrayList CancelJudge(int row, int col, GameData boardData, Game.MoveDir curDir)
	{
		ArrayList cancelRects = new ArrayList();

		RectData rd = null;
		for (int rowIndex  = 0; rowIndex < Game.NumRow; rowIndex++)
		{
			for(int colIndex = 0; colIndex < Game.NumCol; colIndex++)
			{
				rd = DetectRect(rowIndex, colIndex, boardData);
				if (rd != null)
				{
					cancelRects.Add(rd);
					Debug.Log(rd.ToString());
				}
			}
		}
		
		return GetCancelItemsFromRects(cancelRects, boardData);
	}
	
	private ArrayList GetCancelItemsFromRects(ArrayList cancelRects, GameData boardData)
	{
		ArrayList cancelItems = new ArrayList();
		
		foreach(RectData rd in cancelRects)
		{
            for (int row = rd.LeftTop.rowIndex; row <= rd.RightBottom.rowIndex; row++)
			{
                for (int col = rd.LeftTop.colIndex; col <= rd.RightBottom.colIndex; col++)
				{
					if (!cancelItems.Contains(boardData.Data[row,col]))
					{
						cancelItems.Add(boardData.Data[row,col]);
					}
				}
			}
		}
		
		return cancelItems;
	}
	
	public RectData DetectRect(int row,int col, GameData boardData)
	{
		
		int TopHozCount = 0;
		
		ItemData TopLeft = boardData.Data[row,col];
		ItemData TopRight = null;
		ItemData BottomLeft = null;
		ItemData BottomRight = null;
        DataType curType = boardData.Data[row,col].type;
		
		for (int colIndex = col; colIndex < Game.NumCol; colIndex++)
		{
            if (boardData.Data[row,colIndex].type == curType)
			{
				TopHozCount++;
//				TopRight = boardData.Data[row,colIndex];
			}
			else
			{
				break;
			}
		}
//		Debug.Log("TopHozCount:"+TopHozCount);
		
		if (TopHozCount > 1)
		{
            int topHozLength = TopLeft.colIndex + TopHozCount;
            for (int topColIndex = TopLeft.colIndex+1; topColIndex < topHozLength; topColIndex++)
			{
				int LeftVerCount = 0; 
				int RightVerCount = 0;
				TopRight = boardData.Data[row, topColIndex];
				// get left line count
                for (int rowIndex = TopLeft.rowIndex; rowIndex < Game.NumRow; rowIndex++)
				{
                    if (boardData.Data[rowIndex, TopLeft.colIndex].type == curType)
					{
						LeftVerCount++;
					}
					else
					{
						break;
					}
				}
				
				// get right line count
                for (int rowIndex = TopRight.rowIndex; rowIndex < Game.NumRow; rowIndex++)
				{
                    if (boardData.Data[rowIndex, TopRight.colIndex].type == curType)
					{
						RightVerCount++;
					}
					else
					{
						break;
					}
				}
				
				
				int minLength = Math.Min(LeftVerCount, RightVerCount);
//				Debug.Log("left:"+LeftVerCount);
//				Debug.Log("right:"+RightVerCount);
//				Debug.Log("minLength:"+minLength);
				if (minLength > 1)
				{
                    minLength += TopLeft.rowIndex;			
					
                    for (int rowIndex = TopLeft.rowIndex+1; rowIndex < minLength; rowIndex++)
					{
						int colIndex = 0;
						
						Debug.Log("cur:"+rowIndex+":"+colIndex);
                        for (colIndex = TopLeft.colIndex; colIndex < TopRight.colIndex; colIndex++)
						{
                            if (boardData.Data[rowIndex,colIndex].type != curType)
							{
								break;
							}
						}
						
						
                        if (colIndex == TopRight.colIndex)
						{
//							Debug.LogError("Find"+rowIndex+":"+TopRight.ColIndex);
                            BottomLeft = boardData.Data[rowIndex,TopLeft.colIndex];
                            BottomRight = boardData.Data[rowIndex,TopRight.colIndex];
						}
					}
				}
			}
		}
		else
		{
			return null;
		}
		
		if (BottomRight == null)
			return null;
		RectData rd = new RectData();
		
		rd.LeftTop = TopLeft;
		rd.RightBottom = BottomRight;
		
		return rd;
		
	}
}


