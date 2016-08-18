using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using MoveDir = Game.MoveDir;

public class UIManager : MonoBehaviour {

	// public reference
	public GameObject m_ItemPrefab;
    public GameObject m_GamePanel;          //游戏板
	public GameObject m_RestartButton;
    public UILabel		m_lblScore;         //当前分数UI
    public UILabel		m_lblTime;          //时间UI
    public UISprite m_ClockMiddle;          //时间图
    public UISprite		m_GameFrame;        //背景

	public Transform m_ScreenTopLeft;       //屏幕左上位置
    public Transform m_ScreenBottomRight;   //屏幕右下位置    
    public Transform m_ScreenBottomLeft;    //屏幕左下位置
	
	// paused UI
	const float DeltaThresHold = 50;
	Vector3 m_DetectPos = Vector3.zero;
	
	Vector3 m_TopLeft = Vector3.zero;
    Vector3 m_GameBoardTopLeft = Vector3.zero;          //游戏板左上
    Vector3 m_GameBoardBottomRight = Vector3.zero;      //游戏板右下
	
	float m_fBoardTopPadding = 10;
	float m_fBoardRightPadding = 10;
	float m_fBoardBottomPadding = 10; 
	float m_fButtonFrameWidth = 0;
	float m_fButtonFameCenterX = 0;
	
	
	int NumRow = Game.NumRow;
	int NumCol = Game.NumCol;
	
	float m_fItemWidth = 0;
	float m_fItemHeight = 0;
	
	// for type
	string m_CurTypeName = "";
	Dictionary<DataType, string> m_TypeDict = new Dictionary<DataType, string>();
	
    //设定索引器
	#region getter and setter
	public Vector3 TopLeft
	{
		get { return m_TopLeft; }
	}
	
	public Vector3 GameBoardTopLeft
	{
		get { return m_GameBoardTopLeft; }
	}
	
	public Vector3 GameBoardBottonRight
	{
		get { return m_GameBoardBottomRight; }
	}
	
	public float BoardTopPadding
	{
		get { return m_fBoardTopPadding; }
	}
	
	public float ButtonFrameWidth
	{
		get { return m_fButtonFrameWidth; }
	}
	#endregion
	
	
	MoveDir m_CurDir  = MoveDir.None;
	
    //游戏数据
	GameData m_GameData = null;
	GameObject[,] m_Items;
	//添加移动后的item
	ArrayList m_HeadTempItemList = new ArrayList();		// move rigit and up use head list to add temp items
	ArrayList m_TailTempItemList = new ArrayList();		// move left and down use tail list to add temp items

	Vector3 m_CurPressItemOriPos = Vector3.zero;
	float m_fCurMoveDelta = 0;
	int m_CurDeltaNum;
	int m_CurPressRow = 0;
	int m_CurPressCol = 0;
	bool m_bResetPos = false;
	bool m_bIsPaused = false;
	
	private float m_fLeftTime = 60;
	private const float m_TotalTime = 60;
    private int currentScore = 0;
	
    RectCancel m_CancelStratgy = new RectCancel();
	ArrayList m_CurCancelItems;
	bool m_bCanDragItems = true;

	// for audio
	public AudioClip m_WindSound;

	// Use this for initialization
	void Start () {
        InitType();
		m_TopLeft = m_ScreenTopLeft.localPosition;//设置左上位置

        //设定游戏板右下
        m_GameBoardBottomRight = new Vector3(m_ScreenBottomRight.localPosition.x - m_fBoardRightPadding-100, 
		                                     m_ScreenBottomRight.localPosition.y + m_fBoardBottomPadding+50, 0);
		
        //设定格子高宽
        m_fItemHeight = 120;
		m_fItemWidth = m_fItemHeight;
        //设定游戏板左上
		m_GameBoardTopLeft = m_GameBoardBottomRight + new Vector3(-NumCol * m_fItemWidth, NumRow * m_fItemHeight, 0);
		m_fButtonFrameWidth = GameBoardTopLeft.x -  TopLeft.x;
		m_fButtonFameCenterX = TopLeft.x + m_fButtonFrameWidth/2;
		
		
		Game.ItemWidth = m_fItemWidth;
		Game.ItemHeight = m_fItemHeight;
	
        //建立数据和对象数组
		m_GameData = new GameData(NumRow, NumCol);
		m_Items = new GameObject[NumRow,NumCol];
		
        //初始化item
		InitItemsWith(m_GameData.Data);
		
		m_CurCancelItems = m_CancelStratgy.CancelJudge(m_CurPressRow, m_CurPressCol, m_GameData, m_CurDir);	
		CancelEffect(m_CurCancelItems, m_Items);
		
		// init game ui
		UIPanel panel = m_GamePanel.GetComponent<UIPanel>();

        panel.baseClipRegion = new Vector4(m_GameBoardBottomRight.x - NumCol * m_fItemWidth/2, m_GameBoardBottomRight.y + NumRow*m_fItemHeight/2, 
			(m_GameBoardBottomRight.x - m_GameBoardTopLeft.x), 
			(m_GameBoardTopLeft.y - m_GameBoardBottomRight.y));
       
        m_GameFrame.transform.localPosition = new Vector3(m_GameBoardBottomRight.x - NumCol * m_fItemWidth/2, m_GameBoardBottomRight.y + NumRow*m_fItemHeight/2, 0);
        SetItemSize(m_GameFrame,m_GameBoardBottomRight.x - m_GameBoardTopLeft.x + m_fBoardRightPadding*2, 
			m_GameBoardTopLeft.y - m_GameBoardBottomRight.y + m_fBoardTopPadding + m_fBoardBottomPadding);
		
		UIEventListener.Get(m_RestartButton).onClick += OnClickRestart;
	}

    void InitType()
    {
        SetGameItemType("leaves");
    }

    public void SetGameItemType(string typeName)
    {
        if (typeName.Equals(m_CurTypeName))
            return;

        m_CurTypeName = typeName;
        m_TypeDict.Clear();
        List<string> typeList = TypeManager.Instance.GetTypeListByName(typeName);
        if (typeList != null)
        {
            if (Enum.GetNames(typeof(DataType)).Length > typeList.Count)
            {
                Debug.LogError("Not Enough Type");
            }
            else
            {
                for (int i = 0; i < Enum.GetNames(typeof(DataType)).Length; i++)
                {
                    Debug.Log((DataType)i+":"+typeList[i]);
                    m_TypeDict.Add((DataType)i, typeList[i]);
                }
            }
        }
        else
        {
            m_TypeDict.Add(DataType.Green, "green");
            m_TypeDict.Add(DataType.Red, "red");
            m_TypeDict.Add(DataType.Pink, "pink");
        }
    }

	
	void SetItemSize(UISprite spr, float width, float height)
	{
		spr.width = (int)width;
		spr.height = (int)height;
	}
	

	public void RefleshItems()
	{
		GameObject go;
		UISprite img;
		for (int row = 0; row < NumRow; row++)
		{
			for (int col = 0; col < NumCol; col++)
			{
				go = m_Items[row,col];
				// set image
				img = go.transform.FindChild("ItemImg").GetComponent<UISprite>();
				SetItemSize(img, m_fItemWidth, m_fItemHeight);
                img.spriteName = GetSpriteByType(m_GameData.Data[row,col].type);
			}
		}
	}
	
	void Reset()
	{
		
		m_GameData.ReGenerate();
		GameObject go;
		UISprite img;
		for (int row = 0; row < NumRow; row++)
		{
			for (int col = 0; col < NumCol; col++)
			{
				go = m_Items[row,col];
				// set image
				img = go.transform.FindChild("ItemImg").GetComponent<UISprite>();
				SetItemSize(img, m_fItemWidth, m_fItemHeight);
                img.spriteName = GetSpriteByType(m_GameData.Data[row,col].type);
			}
		}
		
		m_CurCancelItems = m_CancelStratgy.CancelJudge(m_CurPressRow, m_CurPressCol, m_GameData, m_CurDir);	
		CancelEffect(m_CurCancelItems, m_Items);
		m_fLeftTime = 60;
		m_lblTime.text = ""+Mathf.CeilToInt(m_fLeftTime);
        m_lblScore.text = "0";
		
	}
	
    /// <summary>
    /// 初始化items
    /// </summary>
	void InitItemsWith(ItemData[,] data)
	{	
		for (int row = 0; row < NumRow; row++)
		{
			for (int col = 0; col < NumCol; col++)
			{
                InitItem(row,col, data[row,col].type);
			}
		}
	}
	
	void SetItemName(GameObject item, string name)
	{
		item.name = name;
	}
	
    /// <summary>
    /// 初始化item
    /// </summary>
	void InitItem(int row, int col, DataType type)
	{
		GameObject go;
		
		go = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
		go.transform.localPosition = new Vector3(col * m_fItemWidth + m_fItemWidth/2,
			-row * m_fItemHeight - m_fItemHeight/2, 0) + m_GameBoardTopLeft;
		go.GetComponent<BoxCollider>().size = new Vector3(m_fItemWidth, m_fItemHeight, 0);
		SetItemName(go, row + "|" + col);
		
		//设置item图片
		UISprite img = go.transform.FindChild("ItemImg").GetComponent<UISprite>();			
		SetItemSize(img, m_fItemWidth, m_fItemHeight);
		img.spriteName = GetSpriteByType(type);
		
		// 添加事件监听
        UIEventListener.Get(go).onPress += OnPressItem; //点击
        UIEventListener.Get(go).onDrag += OnDragItem;   //拖动

		m_Items[row,col] = go;
	}
	
	string GetSpriteByType(DataType type)
	{
//		switch(type)
//		{
//		case ItemData.DataType.Green:
//			return "green";
//		case ItemData.DataType.Orange:
//			return "orange";
//		case ItemData.DataType.Pink:
//			return "pink";
//		default:
//			return "green";
//		}

		return m_TypeDict[type];
	}
	
	void RemoveObjectOfList(ArrayList tempList)
	{
		
		for (int i = 0; i < tempList.Count; i++)
		{
			Destroy(tempList[i] as GameObject);
		}
		
		tempList.Clear();
	}
	
    //item点击监听
	public void OnPressItem(GameObject go, bool state)
	{	
		// pressed
		if (state == true && !m_bResetPos && m_bCanDragItems)
		{
			string[] pos = go.name.Split('|');
			m_CurPressRow = int.Parse(pos[0]);
			m_CurPressCol = int.Parse(pos[1]);
			
			m_CurDeltaNum = 0;
			m_CurDir = MoveDir.None;
			m_fCurMoveDelta = 0;
			m_HeadTempItemList.Clear();
			m_TailTempItemList.Clear();
			m_CurPressItemOriPos = m_Items[m_CurPressRow,m_CurPressCol].transform.localPosition;
		}
		else
		{
			OnPressReleased();
		}
		
		
	}
    //点击释放
	void OnPressReleased()
	{
		if (m_bCanDragItems && !m_bResetPos)
		{
            if (m_CurDir == MoveDir.LeftRight)//左右
			{
				OnLRReleased();
				m_bResetPos = true;
			}
            else if (m_CurDir == MoveDir.UpDown)//上下
			{
				OnUDReleased();
				m_bResetPos = true;
			}
				
			RemoveObjectOfList(m_TailTempItemList);
			RemoveObjectOfList(m_HeadTempItemList);
			
			for(int row = 0; row < NumRow; row++)
			{
				for (int col = 0; col < NumCol; col++)
				{
                    string name = GetSpriteByType(m_GameData.Data[row,col].type);
					string spriteName = m_Items[row,col].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
					
					if (!name.Equals(spriteName))
					{
						Debug.LogError("["+row+","+col+"]"+name);
					}
				}
			}
			
		}
		
		
	}
	
    /// <summary>
    /// Raises the LR released event.
    /// </summary>
	void OnLRReleased()
	{
		float curX = m_Items[m_CurPressRow,m_CurPressCol].transform.localPosition.x;
		float deltaPos = curX - m_CurPressItemOriPos.x;
		int deltaNum = (int)(deltaPos/m_fItemWidth);
		float leftDis = Mathf.Abs(curX - m_CurPressItemOriPos.x) - Mathf.Abs(m_fItemWidth * deltaNum);
//		Debug.Log("leftDis:"+leftDis);
//		Debug.Log("deltaNum:"+deltaNum);
		int count = Mathf.Abs(deltaNum);
				
		if (leftDis > m_fItemWidth/2)
		{
			count += 1;
		}

		// move left
		if (deltaPos < 0 && m_TailTempItemList.Count > 0)
		{
			count = Mathf.Min(count, m_TailTempItemList.Count);
			// swap the game items with temp items
			for (int i = 0; i < count; i++)
			{
				GameObject item = m_Items[m_CurPressRow, i];
				SetItemName(((GameObject)m_TailTempItemList[i]),item.name);
				((GameObject)m_TailTempItemList[i]).GetComponent<BoxCollider>().size = item.GetComponent<BoxCollider>().size; 
				m_Items[m_CurPressRow, i] = (m_TailTempItemList[i] as GameObject);
				UIEventListener.Get(m_Items[m_CurPressRow, i]).onPress += OnPressItem;
				UIEventListener.Get(m_Items[m_CurPressRow, i]).onDrag += OnDragItem;
				m_TailTempItemList[i] = item;
//				Debug.Log("change");
			}
					
			ResetRowIndex(m_CurPressRow, count, true);
		}
		else if (deltaPos > 0 && m_HeadTempItemList.Count > 0)
		{	
			count = Mathf.Min(count, m_HeadTempItemList.Count);
			// swap the game items with temp items
			for (int i = 0; i < count; i++)
			{
				GameObject item = m_Items[m_CurPressRow, NumCol-1-i];
				SetItemName(((GameObject)m_HeadTempItemList[i]) , item.name);
				((GameObject)m_HeadTempItemList[i]).GetComponent<BoxCollider>().size = item.GetComponent<BoxCollider>().size; 
				m_Items[m_CurPressRow, NumCol-1-i] = (m_HeadTempItemList[i] as GameObject);
				UIEventListener.Get(m_Items[m_CurPressRow, NumCol-1-i]).onPress += OnPressItem;
				UIEventListener.Get(m_Items[m_CurPressRow, NumCol-1-i]).onDrag += OnDragItem;
				m_HeadTempItemList[i] = item;
			}
			
			
			ResetRowIndex(m_CurPressRow, count, false);
		}
	}
	
	void OnUDReleased()
	{
		float curY = m_Items[m_CurPressRow,m_CurPressCol].transform.localPosition.y;
		float deltaPos = curY - m_CurPressItemOriPos.y;
		int deltaNum = (int)(deltaPos/m_fItemHeight);
		float leftDis = Mathf.Abs(deltaPos%m_fItemHeight);
//		Debug.Log("leftDis:"+leftDis);
//		Debug.Log("deltaNum:"+deltaNum);
		int count = Mathf.Abs(deltaNum);
		
//		Debug.LogError("count:"+count);
		
		if (leftDis > m_fItemHeight/2)
		{
			count += 1;
		}
			
		// move down
		if (deltaPos < 0 && m_TailTempItemList.Count > 0)
		{
			count = Mathf.Min(count, m_TailTempItemList.Count);
			// swap the game items with temp items
			for (int i = 0; i < count; i++)
			{
				GameObject item = m_Items[NumRow-1-i,m_CurPressCol];
				SetItemName(((GameObject)m_TailTempItemList[i]) , item.name);
				((GameObject)m_TailTempItemList[i]).GetComponent<BoxCollider>().size = item.GetComponent<BoxCollider>().size; 
				m_Items[NumRow-1-i, m_CurPressCol] = (m_TailTempItemList[i] as GameObject);
				UIEventListener.Get(m_Items[NumRow-1-i, m_CurPressCol]).onPress += OnPressItem;
				UIEventListener.Get(m_Items[NumRow-1-i, m_CurPressCol]).onDrag += OnDragItem;
				m_TailTempItemList[i] = item;	
			}
			ResetColIndex(m_CurPressCol, count, true);
		}
		else if (deltaPos > 0 && m_HeadTempItemList.Count > 0)
		{
			count = Mathf.Min(count, m_HeadTempItemList.Count);
			// swap the game items with temp items
			for (int i = 0; i < count; i++)
			{
				GameObject item = m_Items[i, m_CurPressCol];
				SetItemName(((GameObject)m_HeadTempItemList[i]) , item.name);
				((GameObject)m_HeadTempItemList[i]).GetComponent<BoxCollider>().size = item.GetComponent<BoxCollider>().size; 
				m_Items[i, m_CurPressCol] = (m_HeadTempItemList[i] as GameObject);
				UIEventListener.Get(m_Items[i, m_CurPressCol]).onPress += OnPressItem;
				UIEventListener.Get(m_Items[i, m_CurPressCol]).onDrag += OnDragItem;
				m_HeadTempItemList[i] = item;
			}
			ResetColIndex(m_CurPressCol, count, false);
		}
	}
	
	void ResetRowIndex(int row, int count, bool left)
	{
		string nameIndex;
		DataType type;
		GameObject go;
		int resetCount = 0;
		int resetNameCount = 0;
		
		if (left)
		{
			resetCount = NumCol - count;
			resetNameCount = count;
		}
		else
		{
			resetCount = count;
			resetNameCount = NumCol - count;
		}
		
		// correct the items reference
		for (int i = 0; i < resetCount; i++)
		{
			go = m_Items[row, NumCol-1];
            type = m_GameData.Data[row, NumCol-1].type;
			for (int k = NumCol-1; k > 0; k--)
			{
				m_Items[row, k] = m_Items[row, k-1];
                m_GameData.Data[row, k].type = m_GameData.Data[row, k-1].type;
			}
			m_Items[row, 0] = go;
            m_GameData.Data[row, 0].type = type;
		}
		//correct the gameobject name index
		for (int i = 0; i < resetNameCount; i++)
		{
			nameIndex = m_Items[row,NumCol-1].name;
					
			for (int k = NumCol-1; k > 0; k--)
			{
				SetItemName(m_Items[row, k], m_Items[row, k-1].name);		
			}
			
			SetItemName(m_Items[row, 0], nameIndex);		
		}
	}
	
	void ResetColIndex(int col, int count, bool up)
	{	
//		if (count == 0)
//			return;
		
		string nameIndex;
		DataType type;
		GameObject go;
		int resetCount = 0;
		int resetNameCount = 0;
		
		if (up)
		{
			resetCount = count;
			resetNameCount = NumRow - count;		
		}
		else
		{
			resetCount = NumRow - count;
			resetNameCount = count;
		}
		
		// correct the items reference
		for (int i = 0; i < resetCount; i++)
		{
			go = m_Items[NumRow - 1, col];
            type = m_GameData.Data[NumRow-1,col].type;
			for (int k = NumRow - 1; k > 0; k--)
			{
				m_Items[k,col] = m_Items[k-1,col];
                m_GameData.Data[k, col].type = m_GameData.Data[k-1, col].type;
			}
			m_Items[0,col] = go;
            m_GameData.Data[0,col].type = type;
		}
		//correct the gameobject name index
		for (int i = 0; i < resetNameCount; i++)
		{
			nameIndex = m_Items[NumRow-1, col].name;		
			for (int k = NumRow-1; k > 0; k--)
			{
				SetItemName(m_Items[k, col] , m_Items[k-1, col].name);		
			}
			SetItemName(m_Items[0,col], nameIndex);	
		}
	}
	
	void ResetPositionOfRow(int row)
	{
		for (int k = 0; k < NumCol; k++)
		{
			m_Items[row,k].transform.localPosition = new Vector3(k * m_fItemWidth + m_fItemWidth/2,
						-row * m_fItemHeight - m_fItemHeight/2, 0) + m_GameBoardTopLeft;
		}
	}
	
	void ResetPositionOfCol(int col)
	{
		for (int k = 0; k < NumRow; k++)
		{
			m_Items[k,col].transform.localPosition = new Vector3(col * m_fItemWidth + m_fItemWidth/2,
						-k * m_fItemHeight - m_fItemHeight/2, 0) + m_GameBoardTopLeft;
		}
	}
	
	public void OnDragItem(GameObject go, Vector2 delta)
	{
		if (m_bCanDragItems && !m_bResetPos)
		{
			string[] pos = go.name.Split('|');
			int row = int.Parse(pos[0]);
			int col = int.Parse(pos[1]);
			
			//Debug.Log("Click:["+row+","+col+"]");
			if (m_CurDir == MoveDir.None)
			{
				if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
				{
					m_CurDir = MoveDir.LeftRight;
				}
				else
				{
					m_CurDir = MoveDir.UpDown;
				}
			}
			
			if (m_CurDir == MoveDir.LeftRight)
			{
				OnLRDrag(m_CurPressRow, m_CurPressCol, delta);
			}
			else if (m_CurDir == MoveDir.UpDown)
			{
				OnUpDownDrag(m_CurPressRow, m_CurPressCol, delta);
			}
		}
	}
	
	void OnLRDrag(int row, int col, Vector2 delta)
	{
		m_fCurMoveDelta = m_Items[row,col].transform.localPosition.x + delta.x - m_CurPressItemOriPos.x;
//		Debug.Log(m_fCurMoveDelta);
		
		// if move to the end, stop it
		if (Mathf.Abs(m_fCurMoveDelta) < m_fItemWidth * (NumCol - 1))
		{
			// move items with drag delta
			foreach(GameObject temp in m_HeadTempItemList)
			{
				temp.transform.localPosition += new Vector3(delta.x, 0, 0);
			}
			foreach(GameObject temp in m_TailTempItemList)
			{
				temp.transform.localPosition += new Vector3(delta.x, 0, 0);
			}
			
			for (int i = 0; i < NumCol; i++)
			{	
				m_Items[row,i].transform.localPosition += new Vector3(delta.x, 0, 0);		
			}		
			
			// calculate the num of items we moved
			m_CurDeltaNum = Mathf.Abs((int)(m_fCurMoveDelta/m_fItemWidth));
//			Debug.Log("num:"+m_CurDeltaNum);
			if (m_fCurMoveDelta > 0)
			{
				// if we move a harf item, we still need to create one temp items
				if (m_CurDeltaNum + 1 > m_HeadTempItemList.Count)
				{
					GameObject tempItem = null;
					UISprite img = null;
					
					int curTempCount = m_HeadTempItemList.Count;
					
					// if move more passed more than one item position
					for (int k = 0; k < (m_CurDeltaNum + 1 - curTempCount); k++)
					{
						// don't have any temp items when you start to move the items
						if (m_HeadTempItemList.Count == 0)
						{
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = m_Items[row,0].transform.localPosition + new Vector3(-m_fItemWidth, 0, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[row,NumCol-1].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_HeadTempItemList.Add(tempItem);
						}
						else
						{
							GameObject last = m_HeadTempItemList[m_HeadTempItemList.Count - 1] as GameObject;
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = last.transform.localPosition + new Vector3(-m_fItemWidth, 0, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[row,NumCol-1-curTempCount-k].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_HeadTempItemList.Add(tempItem);
						}
					}
				}
			}
			else if(m_fCurMoveDelta < 0)
			{
				// if we move a harf item, we still need to create one temp items
				if (m_CurDeltaNum + 1 > m_TailTempItemList.Count)
				{
					GameObject tempItem = null;
					UISprite img = null;
					
					int curTempCount = m_TailTempItemList.Count;
					
					// if move more passed more than one item position
					for (int k = 0; k < (m_CurDeltaNum + 1 - curTempCount); k++)
					{
						// don't have any temp items when you start to move the items
						if (m_TailTempItemList.Count == 0)
						{
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = m_Items[row,NumCol-1].transform.localPosition + new Vector3(m_fItemWidth, 0, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[row,0].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_TailTempItemList.Add(tempItem);
						}
						else
						{
							GameObject last = m_TailTempItemList[m_TailTempItemList.Count - 1] as GameObject;
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = last.transform.localPosition + new Vector3(m_fItemWidth, 0, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[row,curTempCount+k].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_TailTempItemList.Add(tempItem);
						}
					}
				}
			}
		}// end of side judge
	}
	
	void OnUpDownDrag(int row, int col, Vector2 delta)
	{
		m_fCurMoveDelta = m_Items[row,col].transform.localPosition.y + delta.y - m_CurPressItemOriPos.y;
		
		// if move to the end, stop it
		if (Mathf.Abs(m_fCurMoveDelta) < m_fItemHeight * (NumRow - 1))
		{
			// move items with drag delta
			foreach(GameObject temp in m_HeadTempItemList)
			{
				temp.transform.localPosition += new Vector3(0, delta.y, 0);
			}
			foreach(GameObject temp in m_TailTempItemList)
			{
				temp.transform.localPosition += new Vector3(0, delta.y, 0);
			}
			
			for (int i = 0; i < NumRow; i++)
			{	
				m_Items[i,col].transform.localPosition += new Vector3(0, delta.y, 0);		
			}		

			// calculate the num of items we moved
			m_CurDeltaNum = Mathf.Abs((int)(m_fCurMoveDelta/m_fItemHeight));
	
			if (m_fCurMoveDelta > 0)
			{
				// if we move a harf item, we still need to create one temp items
				if (m_CurDeltaNum + 1 > m_HeadTempItemList.Count)
				{
					GameObject tempItem = null;
					UISprite img = null;
					int curTempCount = m_HeadTempItemList.Count;
					
					// if move more passed more than one item position
					for (int k = 0; k < (m_CurDeltaNum + 1 - curTempCount); k++)
					{
						// don't have any temp items when you start to move the items
						if (m_HeadTempItemList.Count == 0)
						{
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = m_Items[NumRow - 1,col].transform.localPosition + new Vector3(0, -m_fItemHeight, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[0,col].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_HeadTempItemList.Add(tempItem);
						}
						else
						{
							GameObject last = m_HeadTempItemList[m_HeadTempItemList.Count - 1] as GameObject;
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = last.transform.localPosition + new Vector3(0, -m_fItemHeight, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[curTempCount+k,col].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_HeadTempItemList.Add(tempItem);
						}
					}
				}
			}
			else if(m_fCurMoveDelta < 0)
			{
				// if we move a harf item, we still need to create one temp items
				if (m_CurDeltaNum + 1 > m_TailTempItemList.Count)
				{
					GameObject tempItem = null;
					UISprite img = null;
					int curTempCount = m_TailTempItemList.Count;
					
					// if move more passed more than one item position
					for (int k = 0; k < (m_CurDeltaNum + 1 - curTempCount); k++)
					{
						// don't have any temp items when you start to move the items
						if (m_TailTempItemList.Count == 0)
						{
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = m_Items[0,col].transform.localPosition + new Vector3(0, m_fItemHeight, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[NumRow - 1,col].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_TailTempItemList.Add(tempItem);
						}
						else
						{
							GameObject last = m_TailTempItemList[m_TailTempItemList.Count - 1] as GameObject;
							tempItem = NGUITools.AddChild(m_GamePanel, m_ItemPrefab);
							tempItem.transform.localPosition = last.transform.localPosition + new Vector3(0, m_fItemHeight, 0);
							img = tempItem.transform.FindChild("ItemImg").GetComponent<UISprite>();
							SetItemSize(img, m_fItemWidth, m_fItemHeight);
							img.spriteName = m_Items[NumRow-1-curTempCount-k,col].transform.FindChild("ItemImg").GetComponent<UISprite>().spriteName;
							m_TailTempItemList.Add(tempItem);
						}
					}
				}
			}
		}// end of side judge
	}
	
	void CancelEffect(ArrayList cancelItems, GameObject[,] gameObjects)
	{
		m_bCanDragItems = false;
		
		if (cancelItems.Count == 0)
		{;
			m_bCanDragItems = true;
			return;
		}
		
        currentScore += cancelItems.Count;
        m_lblScore.text = "" + currentScore;
		NGUITools.PlaySound(m_WindSound);
		
		GameObject go;
		TweenScale ts;
		ItemData itemData;
		for(int i = 0; i < cancelItems.Count; i++)
		{
			itemData = cancelItems[i] as ItemData;
            go = gameObjects[itemData.rowIndex, itemData.colIndex];
			ts = go.AddComponent<TweenScale>();
			ts.from = new Vector3(1,1,1);
			ts.to 	= new Vector3(0,0,0);
			ts.duration = 0.3f;
			ts.eventReceiver = this.gameObject;
			ts.callWhenFinished = "CancelEnd";
		}
	}
	
	void CancelEnd(UITweener tween)
	{
		string[] pos = tween.gameObject.name.Split('|');
		int row = int.Parse(pos[0]);
		int col = int.Parse(pos[1]);
        m_GameData.Data[row,col].type = m_GameData.GetRandomType();
        InitItem(row, col, m_GameData.Data[row,col].type);
		
		if (RemoveItemFrom(row, col, m_CurCancelItems) == 0)
		{
			m_bCanDragItems = true;
			m_CurCancelItems = m_CancelStratgy.CancelJudge(m_CurPressRow, m_CurPressCol, m_GameData, m_CurDir);
			
			CancelEffect(m_CurCancelItems, m_Items);
		}
		
	}
	
	int RemoveItemFrom(int row, int col, ArrayList cancelItems)
	{
		foreach (ItemData itemData in cancelItems)
		{
            if (itemData.rowIndex == row && itemData.colIndex == col)
			{
				cancelItems.Remove(itemData);
				break;
			}
		}
		
		return cancelItems.Count;
	}
	
	void OnClickRestart(GameObject go)
	{
		NGUITools.SetActive(m_RestartButton, false);
		Reset();
	}
	
	void SetTime(float curTime)
	{
		m_lblTime.text = ""+Mathf.CeilToInt(curTime);
		
		float percent = curTime / m_TotalTime;

		m_ClockMiddle.fillAmount = percent;
	}
	
	
	
	// Update is called once per frame
	void Update () {
		
		if (m_bCanDragItems && !m_bIsPaused)
		{
			m_fLeftTime -= Time.deltaTime;
			if (m_fLeftTime > 0)
			{
				SetTime(m_fLeftTime);
			}
			else
			{
				// time out
				SetTime(0);
				m_bCanDragItems = false;
				NGUITools.SetActive(m_RestartButton, true);
				
			}
			
			
		}
		
		if (m_bResetPos)
		{
			m_bCanDragItems = false;
			if (m_CurDir == MoveDir.LeftRight)
			{
				if (!Game.IsClose(m_Items[m_CurPressRow, 0].transform.localPosition.x, m_GameBoardTopLeft.x + m_fItemWidth/2, 0.1f))
				{
					Vector3 curPos;
					float curX = 0;
					for (int k = 0; k <NumCol; k++)
					{
						curPos = m_Items[m_CurPressRow, k].transform.localPosition;
						curX = curPos.x;
						curX = Mathf.Lerp(curX, m_GameBoardTopLeft.x + k * m_fItemWidth + m_fItemWidth/2, Time.deltaTime * 15);
						m_Items[m_CurPressRow, k].transform.localPosition = new Vector3(curX, curPos.y, curPos.z);
//							m_Items[m_CurPressRow, k].transform.localPosition = Vector3.Lerp(curPos, new Vector3(k * m_fItemWidth + m_fItemWidth/2,
//													curPos.y, curPos.z) + m_GameBoardTopLeft, Time.deltaTime);
					}
				}
				else
				{
					ResetPositionOfRow(m_CurPressRow);
					m_bResetPos = false;

					m_CurCancelItems = m_CancelStratgy.CancelJudge(m_CurPressRow, m_CurPressCol, m_GameData, m_CurDir);
					if (m_CurCancelItems.Count > 0)
					{
						CancelEffect(m_CurCancelItems, m_Items);
						//NGUITools.PlaySound(m_WindSound);
					}
					else
					{
						m_bCanDragItems = true;
					}
				}
			}
			else if (m_CurDir == MoveDir.UpDown)
			{
	
				if (!Game.IsClose(m_Items[0, m_CurPressCol].transform.localPosition.y, m_GameBoardTopLeft.y - m_fItemHeight/2, 0.1f))
				{
					Vector3 curPos;
					float curY = 0;
					for (int k = 0; k <NumRow; k++)
					{
						curPos = m_Items[k, m_CurPressCol].transform.localPosition;
						curY = curPos.y;
						curY = Mathf.Lerp(curY, m_GameBoardTopLeft.y - k * m_fItemHeight - m_fItemHeight/2, Time.deltaTime * 15);
						m_Items[k, m_CurPressCol].transform.localPosition = new Vector3(curPos.x, curY, curPos.z);
					}
				}
				else
				{
					ResetPositionOfCol(m_CurPressCol);
					m_bResetPos = false;

					m_CurCancelItems = m_CancelStratgy.CancelJudge(m_CurPressRow, m_CurPressCol, m_GameData, m_CurDir);
					if (m_CurCancelItems.Count > 0)
					{				
						CancelEffect(m_CurCancelItems, m_Items);
						//NGUITools.PlaySound(m_WindSound);
					}
					else
					{
						m_bCanDragItems = true;
					}
				}
			}
		}
	
		
		if (Input.GetKeyUp(KeyCode.Escape)) 
		{    
		    Debug.Log("Return Button");
			Application.Quit();
		}

	}
}
