using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GridCoordinates {
	public int Row;
	public int Column;
	
	public GridCoordinates() { }
	public GridCoordinates(int row, int column) {
		Row = row;
		Column = column;
	}
	
	public override string ToString() {
		return string.Format ("R: {0} C: {1}", Row, Column);
	}
}

public class GridSquare{
	public GridCoordinates GridCoords = new GridCoordinates();
	public float X;
	public float Y;
	public bool IsTraversable;
	public GridComponent Component = null;
	public GridConsumable Consumable = null;
	public Actor Occupier = null;
	
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
	
	public bool IsOccupied() {
		return Occupier != null; 
	}
	
	public override string ToString() {
		return string.Format ("{0} X: {1} Y: {2} T: {3}", GridCoords, X, Y, IsTraversable);
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
	/// Sets theGridComponent for a grid square.
	/// </summary>
	/// <param name='coords'>
	/// Coords.
	/// </param>
	/// <param name='component'>
	/// Component.
	/// </param>
	public void SetComponent(GridCoordinates coords, GridComponent component) {
		SquarePositions[coords.Row][coords.Column].Component = component;
	}
	
	/// <summary>
	/// Assigns a consumable to a square.
	/// </summary>
	/// <param name='coords'>
	/// Coords.
	/// </param>
	/// <param name='consumable'>
	/// Consumable.
	/// </param>
	public void SetConsumable(GridCoordinates coords, GridConsumable consumable) {
		SquarePositions[coords.Row][coords.Column].Consumable = consumable;	
	}
	
	/// <summary>
	/// Determines whether a grid square is traversable
	/// </summary>
	/// <returns>
	/// <c>true</c> if the square is traversable, otherwise <c>false</c>. If no square is found, false is returned.
	/// </returns>
	/// <param name='row'>
	/// The row number.
	/// </param>
	/// <param name='column'>
	/// The column number.
	/// </param>Che
	public bool IsTraversableSquare(int row, int column) {
		if (column < NumColumns && column >= 0 && row < NumRows && row >= 0) {
			return SquarePositions[row][column].IsTraversable;
		}
		else {
			return false;
		}
	}
}
