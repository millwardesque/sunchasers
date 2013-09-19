using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GridCoordinates {
	public int Row;
	public int Column;
	
	public override string ToString() {
		return "R: " + Row + " C: " + Column;
	}
}

public class GridSquare{
	public GridCoordinates GridCoords = new GridCoordinates();
	public float X;
	public float Y;
	public bool IsTraversable;
	public List<GridComponent> Components = new List<GridComponent>();
	
	public int Row {
		get { return GridCoords.Row; }
		set { GridCoords.Row = value; }
	}
	
	public int Column {
		get { return GridCoords.Column; }
		set { GridCoords.Column = value; }
	}
	
	public GridSquare() {
		this.Row = 0;
		this.Column = 0;
		this.X = 0.0f;
		this.Y = 0.0f;
		this.IsTraversable = true;
	}
	
	public GridSquare(int row, int column, float x, float y, bool isTraversable) {
		Row = row;
		Column = column;
		X = x;
		Y = y;
		IsTraversable = isTraversable;
	}
	
	public override string ToString() {
		return GridCoords + " X: " + X + " Y: " + Y + " T: " + IsTraversable;
	}
}

public class MovementGrid : MonoBehaviour {
	public int NumRows = 4;
	public int NumColumns = 17;
	public float GridSquareWidth = 3.75f;
	public float GridSquareHeight = 6.0f;
	public List< List<GridSquare> > SquarePositions = new List< List<GridSquare> >();
	public GridCoordinates[] DisabledSquares;
	
	/// <summary>
	/// Awake hook.
	/// </summary>
	void Awake () {
		float startOffsetX = 1.1f;
		float startOffsetY = 1.0f;
		
		// Create the grid
		for (int y = 0; y < this.NumRows; ++y) {
			List<GridSquare> row = new List<GridSquare>();
			SquarePositions.Add(row);
			
			for (int x = 0; x < this.NumColumns; ++x) {
				float pixelX = x * GridSquareWidth + startOffsetX;
				float pixelY = y * GridSquareHeight + startOffsetY;
				
				row.Add(new GridSquare(y, x, pixelX, pixelY, true));
			}
		}
		
		// Disable the appropriate squares
		for (int i = 0; i < DisabledSquares.Length; ++i) {
			GridCoordinates square = DisabledSquares[i];
			SquarePositions[square.Row][square.Column].IsTraversable = false;
		}
	}
	
	/// <summary>
	/// Adds a GridComponent to a grid square.
	/// </summary>
	/// <param name='coords'>
	/// Coords.
	/// </param>
	/// <param name='component'>
	/// Component.
	/// </param>
	public void AddToGridSquare(GridCoordinates coords, GridComponent component) {
		Debug.Log("Added component to grid");
		SquarePositions[coords.Row][coords.Column].Components.Add(component);
	}
}
