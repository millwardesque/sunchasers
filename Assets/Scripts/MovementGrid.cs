﻿using UnityEngine;
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
	
	/// <summary>
	/// Gets the distance to a destination in grid squares.
	/// </summary>
	/// <param name='destination'>
	/// Destination.
	/// </param>
	public int DistanceTo(GridCoordinates destination) {
		return Mathf.Abs(Row - destination.Row) + Mathf.Abs(Column - destination.Column);
	}
	
	public bool Equals(GridCoordinates coords) {
		if (coords == null) {
			return false;
		}
		return (Row == coords.Row && Column == coords.Column);
	}
	
	public override bool Equals(System.Object obj) {
		if (obj == null) {
			return false;
		}
		
		GridCoordinates coords = obj as GridCoordinates;
		return (Row == coords.Row && Column == coords.Column);
	}
	
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}

class AStarGridSquare {
	public GridCoordinates Coords;
	int local_g = 10;
	public AStarGridSquare Parent = null;
	public GridCoordinates Target = null;
	public bool IsTraversable = true;
	
	public int Row { get { return Coords.Row; } }
	public int Column { get { return Coords.Column; } }
	
	public int F {
		get { return G + H; }
	}
	
	public int G {
		get {
			int score = local_g;
			if (null != Parent) {
				score += Parent.G;
			}
			return score;
		}
	}
	
	public int H {
		get {
			return Coords.DistanceTo(Target) * 10;
		}
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="AStarGridSquare"/> class.
	/// </summary>
	/// <param name='coords'>
	/// Coords.
	/// </param>
	/// <param name='parent'>
	/// Parent.
	/// </param>
	/// <param name='isTraversable'>
	/// Is traversable.
	/// </param>
	public AStarGridSquare(GridCoordinates coords, GridCoordinates target, AStarGridSquare parent, bool isTraversable) {
		Coords = coords;
		Parent = parent;
		Target = target;
		IsTraversable = isTraversable;
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

	public Vector2 PixelCoords {
		get { return new Vector2(X, Y); }
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
	/// Sets the GridComponent for a grid square.
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

	/// <summary>
	/// Gets the grid square from the world-space position.
	/// </summary>
	/// <returns>The grid square at the given position.</returns>
	/// <param name="position">World-space Position.</param>
	public GridSquare GetSquareFromPosition(Vector3 position) {
		float closestDistance = 9999999;
		GridSquare closestSquare = null;

		// Convert the position to localspace.
		position -= transform.position;

		// Find the closest square.
		for (int row = 0; row < NumRows; row++) {
			for (int column = 0; column < NumColumns; column++) {
				float distance = (new Vector2(position.x, position.y) - SquarePositions[row][column].PixelCoords).magnitude;
				if (distance < closestDistance) {
					closestDistance = distance;
					closestSquare = SquarePositions[row][column];
				}
			}
		}

		return closestSquare;
	}

	public bool SquareHasChair(int row, int column) {
		if (column < NumColumns && column >= 0 && row < NumRows && row >= 0 && SquarePositions[row][column].Component != null) {
			return (SquarePositions[row][column].Component.tag == "Chair");
		}
		else {
			return false;
		}
	}
	
	/// <summary>
	/// Finds the path to a grid square.
	/// </summary>
	/// <returns>
	/// A list of grid-squares on the path, including the start and end squares.
	/// </returns>
	/// <param name='start'>
	/// The start square.
	/// </param>
	/// <param name='end'>
	/// The end square.
	/// </param>
	public List<GridCoordinates> FindPathToSquare(GridCoordinates start, GridCoordinates end) {
		List<AStarGridSquare> openList = new List<AStarGridSquare>();
		List<AStarGridSquare> closedList = new List<AStarGridSquare>();
		AStarGridSquare foundEnd = null;

		if (start.Equals(end)) {
			return new List<GridCoordinates>();
		}
		
		openList.Add(MakeAStarSquare(start.Row, start.Column, end, null));
		while (openList.Count > 0) {
			// Pop the best square.
			AStarGridSquare currentSquare = FindBestFScore(openList);
			closedList.Add(currentSquare);
			openList.Remove(currentSquare);
			
			if (currentSquare.Row == end.Row && currentSquare.Column == end.Column) {
				foundEnd = currentSquare;
				break;
			}
			
			List<AStarGridSquare> adjacentSquares = GetAdjacentTraversableSquares(currentSquare.Row, currentSquare.Column, end, currentSquare);
			foreach (AStarGridSquare square in adjacentSquares) {
				if (closedList.Find( x => x.Row == square.Row && x.Column == square.Column) == null) {
					AStarGridSquare openSquare = openList.Find( x => x.Row == square.Row && x.Column == square.Column);
					if (null != openSquare) {
						if (openSquare.G >= square.G) {
							openSquare.Parent = square;
						}
					}
					else {
						openList.Add(square);	
					}
				}
			}
		}
		
		if (foundEnd != null) {
			List<GridCoordinates> path = new List<GridCoordinates>();
			do {
				path.Add(new GridCoordinates(foundEnd.Row, foundEnd.Column));
				foundEnd = foundEnd.Parent;
			} while (foundEnd.Parent != null);
			path.Reverse();

			return path;
		}
		else {				
			return null;
		}
	}
	
	/// <summary>
	/// Makes a new AStarSquare based on the movement grid.
	/// </summary>
	/// <returns>
	/// The A star square.
	/// </returns>
	/// <param name='row'>
	/// Row.
	/// </param>
	/// <param name='column'>
	/// Column.
	/// </param>
	private AStarGridSquare MakeAStarSquare(int row, int column, GridCoordinates target, AStarGridSquare parent) {
		if (row >= 0 && row < NumRows && column >= 0 && column < NumColumns) {
			GridSquare square = SquarePositions[row][column];
			return new AStarGridSquare(new GridCoordinates(row, column), target, parent, square.IsTraversable);
		}
		else {
			return null;
		}
	}
	
	/// <summary>
	/// Gets the adjacent traversable squares.
	/// </summary>
	/// <returns>
	/// The adjacent squares.
	/// </returns>
	/// <param name='row'>
	/// Row.
	/// </param>
	/// <param name='column'>
	/// Column.
	/// </param>
	private List<AStarGridSquare> GetAdjacentTraversableSquares(int row, int column, GridCoordinates target, AStarGridSquare parent) {
		List<AStarGridSquare> adjacentSquares = new List<AStarGridSquare>();
		if (row - 1 >= 0 && IsTraversableSquare(row - 1, column) && !SquareHasChair(row - 1, column)) {			// Southern square.
			adjacentSquares.Add(MakeAStarSquare(row - 1, column, target, parent));
		}
		if (row + 1 < NumRows && IsTraversableSquare(row + 1, column) && !SquareHasChair(row, column)) {	// Northern square.
			adjacentSquares.Add(MakeAStarSquare(row + 1, column, target, parent));
		}
		if (column - 1 >= 0 && IsTraversableSquare(row, column - 1)) {		// Western square.
			adjacentSquares.Add(MakeAStarSquare(row, column - 1, target, parent));
		}
		if (column + 1 < NumColumns && IsTraversableSquare(row, column + 1)) {	// Eastern square.
			adjacentSquares.Add(MakeAStarSquare(row, column + 1, target, parent));
		}
		
		return adjacentSquares;
	}
	
	/// <summary>
	/// Finds the square with the best F score.
	/// </summary>
	/// <returns>
	/// The best F score.
	/// </returns>
	/// <param name='squares'>
	/// Squares.
	/// </param>
	private AStarGridSquare FindBestFScore(List<AStarGridSquare> squares) {
		AStarGridSquare best = null;
		foreach (AStarGridSquare square in squares) {
			if (best == null || square.F <= best.F) {
					best = square;
			}
		}
		
		return best;
	}

	/// <summary>
	/// Debug member function for dumping the path found by the A* algorithm.
	/// </summary>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	/// <param name="path">Path.</param>
	private void DumpPath(GridCoordinates start, GridCoordinates end, List<GridCoordinates> path) {
		Debug.Log (string.Format ("Start: {0}", start));
		int index = 1;
		foreach (GridCoordinates step in path) {
			Debug.Log (string.Format ("{0}: {1}", index, step));
			index++;
		}
		Debug.Log (string.Format ("End: {0}", end));
	}
}
