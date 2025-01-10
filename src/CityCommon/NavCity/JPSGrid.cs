using System.Collections.Generic;
using System;
using System.Linq;


//TODO动态局部更新grid
//TODO 带权


public enum eDirections
{
	NORTH      = 0,
	NORTH_EAST = 1,
	EAST       = 2,
	SOUTH_EAST = 3,
	SOUTH      = 4,
	SOUTH_WEST = 5,
	WEST       = 6,
	NORTH_WEST = 7,
}

public class Node
{
	public Point pos;
	public bool isObstacle = false;
	public int[] jpDistances = new int[8];
    public float cost = 1;

	public bool isJumpPoint = false;
	// Holds if primary jump point has direction COMING FROM the Cardianal direction,
	// so jumpPointDirection[ EAST ] means it's a jump point for paths COMING FROM THE EAST 
	// Note: This would be "Moving Left" in Steve Rabin's implementation
	public bool[] jumpPointDirection = new bool[8];

	public bool isJumpPointComingFrom( eDirections dir )
	{
		return this.isJumpPoint && this.jumpPointDirection[ (int) dir ];
	}
}

public enum PathStatus
{
	ON_NONE,
	ON_OPEN,
	ON_CLOSED
}

public class PathfindingNode
{
	public PathfindingNode parent;
	public Point pos;
	public int givenCost;
	public int finalCost;
	public eDirections directionFromParent;
	public PathStatus pathStatus = PathStatus.ON_NONE;

	public void Reset()
	{
		this.parent = null;
		this.givenCost = 0;
		this.finalCost = 0;
		this.pathStatus = PathStatus.ON_NONE;
	}
}

public class JPSGrid
{
	public Node[] gridNodes = new Node[0];
	private int w;
	private int h;


	private static Dictionary<eDirections, eDirections[]> validDirLookUpTable;

	private static eDirections[] allDirections = Enum.GetValues(typeof(eDirections)).Cast<eDirections>().ToArray();

	//静态构造函数
	static JPSGrid()
    {
		validDirLookUpTable = new Dictionary<eDirections, eDirections[]>();
		validDirLookUpTable[eDirections.SOUTH] = new[] { eDirections.WEST, eDirections.SOUTH_WEST, eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST };
		validDirLookUpTable[eDirections.SOUTH_EAST] = new[] { eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST };
		validDirLookUpTable[eDirections.EAST] = new[] { eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST, eDirections.NORTH_EAST, eDirections.NORTH };
		validDirLookUpTable[eDirections.NORTH_EAST] = new[] { eDirections.EAST, eDirections.NORTH_EAST, eDirections.NORTH };
		validDirLookUpTable[eDirections.NORTH] = new[] { eDirections.EAST, eDirections.NORTH_EAST, eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST };
		validDirLookUpTable[eDirections.NORTH_WEST] = new[] { eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST };
		validDirLookUpTable[eDirections.WEST] = new[] { eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST, eDirections.SOUTH_WEST, eDirections.SOUTH };
		validDirLookUpTable[eDirections.SOUTH_WEST] = new[] { eDirections.WEST, eDirections.SOUTH_WEST, eDirections.SOUTH };
	}

	public JPSGrid(MapData<byte> data):this(data, new MyRectInt(0,0,data.w,data.h))
	{

	}

	public bool IsWalkable(int x, int y)
	{
		return !gridNodes[x + y * w].isObstacle;
	}

	public JPSGrid(MapData<byte> data, MyRectInt rect)
	{
		this.w = rect.width; 
		this.h = rect.height;
		int numBlocks = w * h;
		gridNodes = new Node[numBlocks];
		int x1 = rect.xMin;
		int y1 = rect.yMin;
		int x2 = rect.xMax;
		int y2 = rect.yMax;

		int index = 0;
		for(int y = y1; y < y2; y++)
        {
			for(int x= x1; x < x2; x++)
			{
				Node node = new Node();
				node.pos = new Point(x-x1, y-y1);
				node.isObstacle = data[x, y] != TerrainMap.LAND_MIN_ID;
				gridNodes[index++] = node;
			}
        }

		Prepare();
	}

	private void Prepare()
	{
		TickLog.Start("Prepare JPSGrid");
		buildPrimaryJumpPoints();
		buildStraightJumpPoints();
		buildDiagonalJumpPoints();
		TickLog.End("Prepare JPSGrid");
	}

	static string dirToStr( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.NORTH:
				return "NORTH";
			case eDirections.NORTH_EAST:
				return "NORTH_EAST";
			case eDirections.EAST:
				return "EAST";
			case eDirections.SOUTH_EAST:
				return "SOUTH_EAST";
			case eDirections.SOUTH:
				return "SOUTH";
			case eDirections.SOUTH_WEST:
				return "SOUTH_WEST";
			case eDirections.WEST:
				return "WEST";
			case eDirections.NORTH_WEST:
				return "NORTH_WEST";
		}

		return "NONE";
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getNorthEastIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column + 1 >= w || row - 1 < 0 ) return -1;

		return ( column + 1 ) +
			( row - 1 ) * w;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getSouthEastIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column + 1 >= w || row + 1 >= h ) return -1;

		return ( column + 1 ) +
			( row + 1 ) * w;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getSouthWestIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column - 1 < 0 || row + 1 >= h ) return -1;

		return ( column - 1 ) +
			( row + 1 ) * w;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getNorthWestIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column - 1 < 0 || row - 1 < 0 ) return -1;

		return ( column - 1 ) +
			( row - 1 ) * w;
	}

	private int rowColumnToIndex ( int row, int column )
	{
		return column + ( row * w );
	}

	private int pointToIndex( Point pos )
	{
		return rowColumnToIndex( pos.y, pos.x );
	}

	private bool isEmpty( int index )
	{
		if ( index < 0 ) return false;

		int row, column;
		row    = index / w;
		column = index % w;

		return isEmpty( row, column );
	}

	private bool isObstacleOrWall( int index )
	{
		if ( index < 0 ) return true;

		int row, column;
		row    = index / w;
		column = index % w;

		return isObstacleOrWall( row, column );
	}

	private bool isEmpty( int row, int column )
	{
		return ! isObstacleOrWall( row, column );
	}

	private bool isObstacleOrWall( int row, int column )
	{
		// If we are out of bounds, then we are def a wall
		return isInBounds( row, column ) && gridNodes[ column + ( row * w ) ].isObstacle;  
	}

	private bool isJumpPoint( int row, int column, eDirections dir )
	{
		if ( isInBounds( row, column ) )
		{
			Node node = gridNodes[ column + ( row * w ) ];
			return node.isJumpPoint && node.jumpPointDirection[ (int) dir ];
		}

		return false;  // If we are out of bounds, then we are def a wall
	}

	private bool isInBounds( int index )
	{
		if ( index < 0 || index >= gridNodes.Length ) return false;

		int row, column;
		row    = index / w;
		column = index % w;

		return isInBounds( row, column );
	}

	private bool isInBounds( int row, int column )
	{
		return row >= 0 && row < h && column >= 0 && column < w;
	}

	// Returns Grid Index of node in the given direction
	// Returns -1 if index is out of bounds
	private int getIndexOfNodeTowardsDirection( int index, eDirections direction )
	{
		int row, column;
		row    = index / w;
		column = index % w;

		int change_row    = 0;
		int change_column = 0;

		// Change in the Row Direction
		switch ( direction )
		{
			case eDirections.NORTH_EAST:
			case eDirections.NORTH:
			case eDirections.NORTH_WEST:
				change_row = -1;
				break;

			case eDirections.SOUTH_EAST:
			case eDirections.SOUTH:
			case eDirections.SOUTH_WEST:
				change_row = 1;
				break;
		}

		// Change in the Column Direction
		switch ( direction )
		{
			case eDirections.NORTH_EAST:
			case eDirections.EAST:
			case eDirections.SOUTH_EAST:
				change_column = 1;
				break;

			case eDirections.SOUTH_WEST:
			case eDirections.WEST:
			case eDirections.NORTH_WEST:
				change_column = -1;
				break;
		}

		// Calc new rows and columns
		int new_row    = row    + change_row;
		int new_column = column + change_column;

		// Check bounds
		if ( isInBounds( new_row, new_column ) )
		{
			return new_column + ( new_row * w );
		}

		return -1;    // Out of bounds is -1
	}

	private void buildPrimaryJumpPoints()
	{
		// foreach obstacle
		for ( int i = 0 ; i < gridNodes.Length ; ++i )
		{
			Node current_node = gridNodes[ i ];

			// find forced neighbor scenarios
			if (current_node.isObstacle )
			{
				int row, column;
				row    = i / w;
				column = i % w;

				// Check Diagonal Nodes to see if they are also obstacles
				int north_east_index, south_east_node, south_west_node, north_west_node;

				// North East
				north_east_index = getNorthEastIndex( row, column );

				if ( north_east_index != -1 )
				{
					Node node = gridNodes[ north_east_index ];

					if ( ! node.isObstacle )
					{
						// If nodes to the south and west are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.SOUTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.WEST  ] = true;
						}
					}
				}

				// South East
				south_east_node = getSouthEastIndex( row, column );

				if ( south_east_node != -1  )
				{
					Node node = gridNodes[ south_east_node ];

					if ( ! node.isObstacle )
					{
						// If nodes to the north and west are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.NORTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.WEST  ] = true;
						}
					}
				}

				// South West
				south_west_node = getSouthWestIndex( row, column );

				if ( south_west_node != -1  )
				{
					Node node = gridNodes[ south_west_node ];

					if ( ! node.isObstacle )
					{
						// If nodes to the north and East are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.NORTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.EAST  ] = true;
						}
					}
				}

				// North West
				north_west_node = getNorthWestIndex( row, column );

				if ( north_west_node != -1  )
				{
					Node node = gridNodes[ north_west_node ];

					if ( ! node.isObstacle )
					{
						// If nodes to the south and East are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.SOUTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.EAST  ] = true;
						}
					}
				}

			}
		}
	}

	private void buildStraightJumpPoints()
	{
		// Calcin' Jump Distance, left and right
		// For all the rows in the grid
		for ( int row = 0 ; row < h ; ++row )
		{
			// Calc moving left to right
			int  jumpDistanceSoFar = -1;
			bool jumpPointSeen = false;

			// Checking for jump disances where nodes are moving WEST
			for ( int column = 0 ; column < w ; ++column )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					node.jpDistances[ (int) eDirections.WEST ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
					node.jpDistances[ (int) eDirections.WEST ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.WEST ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.EAST ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}

			jumpDistanceSoFar = -1;
			jumpPointSeen = false;
			// Checking for jump disances where nodes are moving WEST
			for ( int column = w - 1 ; column >= 0 ; --column )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					node.jpDistances[ (int) eDirections.EAST ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his RIGTH ( EAST )
					node.jpDistances[ (int) eDirections.EAST ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.EAST ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.WEST ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}
		}

		// Calcin' Jump Distance, up and down
		// For all the columns in the grid
		for ( int column = 0 ; column < w ; ++column )
		{
			// Calc moving left to right
			int  jumpDistanceSoFar = -1;
			bool jumpPointSeen = false;

			// Checking for jump disances where nodes are moving NORTH
			for ( int row = 0 ; row < h ; ++row )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					node.jpDistances[ (int) eDirections.NORTH ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading UP, then we can tell this node he's got a jump point coming up ABOVE ( NORTH )
					node.jpDistances[ (int) eDirections.NORTH ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.NORTH ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.SOUTH ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}

			jumpDistanceSoFar = -1;
			jumpPointSeen = false;
			// Checking for jump disances where nodes are moving SOUTH
			for ( int row = h - 1 ; row >= 0 ; --row )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					node.jpDistances[ (int) eDirections.SOUTH ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading down, then we can tell this node he's got a jump point coming up BELOW( SOUTH )
					node.jpDistances[ (int) eDirections.SOUTH ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.SOUTH ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.NORTH ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}
		}
	}

	private Node getNode( int row, int column )
	{
		Node node = null;

		if ( isInBounds( row, column ) )
		{
			node = gridNodes[ rowColumnToIndex( row, column ) ];
		}

		return node;
	}

	private void buildDiagonalJumpPoints()
	{
		// Calcin' Jump Distance, Diagonally Upleft and upright
		// For all the rows in the grid
		for ( int row = 0 ; row < h ; ++row )
		{
			// foreach column
			for ( int column = 0 ; column < w ; ++column )
			{
				// if this node is an obstacle, then skip
				if ( isObstacleOrWall( row, column ) ) continue;
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];    // Grab the node ( will not be NULL! )

				// Calculate NORTH WEST DISTNACES
				if ( row  == 0 || column == 0 || (                  // If we in the north west corner
					isObstacleOrWall( row - 1, column ) ||          // If the node to the north is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the left is an obstacle
					isObstacleOrWall( row - 1, column - 1 ) ) )     // if the node to the North west is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.NORTH_WEST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                    // if the node to the north is empty
					isEmpty(row, column - 1) &&                                                          // if the node to the west is empty
					(getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.NORTH ] > 0 ||    // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.WEST  ] > 0))     // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.NORTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.NORTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.NORTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.NORTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate NORTH EAST DISTNACES
				if ( row  == 0 || column == w -1 || (         // If we in the top right corner
					isObstacleOrWall( row - 1, column ) ||          // If the node to the north is an obstacle
					isObstacleOrWall( row, column + 1) ||           // If the node to the east is an obstacle
					isObstacleOrWall( row - 1, column + 1 ) ) )     // if the node to the North East is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.NORTH_EAST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                    // if the node to the north is empty
					isEmpty(row, column + 1) &&                                                          // if the node to the east is empty
					(getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.NORTH ] > 0 ||    // If the node to the north east has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.EAST  ] > 0))     // If the node to the north east has is a straight jump point ( or primary jump point) going east
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.NORTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.NORTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.NORTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.NORTH_EAST ] = -1 + jumpDistance;
					}
				}
			}
		}

		// Calcin' Jump Distance, Diagonally DownLeft and Downright
		// For all the rows in the grid
		for ( int row = h - 1 ; row >= 0 ; --row )
		{
			// foreach column
			for ( int column = 0 ; column < w ; ++column )
			{
				// if this node is an obstacle, then skip
				if ( isObstacleOrWall( row, column ) ) continue;
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];    // Grab the node ( will not be NULL! )

				// Calculate SOUTH WEST DISTNACES
				if ( row == h - 1 || column == 0 || (         // If we in the south west most node
					isObstacleOrWall( row + 1, column ) ||          // If the node to the south is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the west is an obstacle
					isObstacleOrWall( row + 1, column - 1 ) ) )     // if the node to the south West is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                    // if the node to the south is empty
					isEmpty(row, column - 1) &&                                                          // if the node to the west is empty
					(getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.SOUTH ] > 0 ||    // If the node to the south west has is a straight jump point ( or primary jump point) going south
					 getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.WEST  ] > 0))     // If the node to the south west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.SOUTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.SOUTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate SOUTH EAST DISTNACES
				if ( row  == h - 1 || column == w -1 || (    // If we in the south east corner
					isObstacleOrWall( row + 1, column ) ||               // If the node to the south is an obstacle
					isObstacleOrWall( row, column + 1) ||                // If the node to the east is an obstacle
					isObstacleOrWall( row + 1, column + 1 ) ) )          // if the node to the south east is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                    // if the node to the south is empty
					isEmpty(row, column + 1) &&                                                          // if the node to the east is empty
					(getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.SOUTH ] > 0 ||    // If the node to the south east has is a straight jump point ( or primary jump point) going south
					 getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.EAST  ] > 0))     // If the node to the south east has is a straight jump point ( or primary jump point) going east
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.SOUTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.SOUTH_EAST ] = -1 + jumpDistance;
					}
				}
			}
		}
	}

	static readonly float SQRT_2 = (float)Math.Sqrt( 2 );
	static readonly float SQRT_2_MINUS_1 = (float)Math.Sqrt( 2 ) - 1.0f;

    internal static int octileHeuristic( int curr_row, int curr_column, int goal_row, int goal_column )
	{
		int heuristic;
		int row_dist = goal_row - curr_row;
		int column_dist = goal_column - curr_column;

		heuristic = (int) ( Math.Max( row_dist, column_dist ) + SQRT_2_MINUS_1 * Math.Min( row_dist, column_dist ) );

		return heuristic;
	}

	private eDirections[] getAllValidDirections( PathfindingNode curr_node )
	{
		// If parent is null, then explore all possible directions
		return curr_node.parent == null ? 
			allDirections : 
			validDirLookUpTable[ curr_node.directionFromParent ];
	}

	private bool isCardinal( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.SOUTH:
			case eDirections.EAST:
			case eDirections.NORTH:
			case eDirections.WEST:
				return true;
		}

		return false;
	}

	private bool isDiagonal( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.SOUTH_EAST:
			case eDirections.SOUTH_WEST:
			case eDirections.NORTH_EAST:
			case eDirections.NORTH_WEST:
				return true;
		}

		return false;
	}

	private bool goalIsInExactDirection( Point curr, eDirections dir, Point goal )
	{
		int diff_column = goal.x - curr.x;
		int diff_row    = goal.y - curr.y;

		// note: north would be DECREASING in row, not increasing. Rows grow positive while going south!
		switch ( dir )
		{
			case eDirections.NORTH:
				return diff_row < 0 && diff_column == 0;
			case eDirections.NORTH_EAST:
				return diff_row < 0 && diff_column > 0 && Math.Abs(diff_row) == Math.Abs(diff_column);
			case eDirections.EAST:
				return diff_row == 0 && diff_column > 0;
			case eDirections.SOUTH_EAST:
				return diff_row > 0 && diff_column > 0 && Math.Abs(diff_row) == Math.Abs(diff_column);
			case eDirections.SOUTH:
				return diff_row > 0 && diff_column == 0;
			case eDirections.SOUTH_WEST:
				return diff_row > 0 && diff_column < 0 && Math.Abs(diff_row) == Math.Abs(diff_column);
			case eDirections.WEST:
				return diff_row == 0 && diff_column < 0;
			case eDirections.NORTH_WEST:
				return diff_row < 0 && diff_column < 0 && Math.Abs(diff_row) == Math.Abs(diff_column);
		}

		return false;
	}

	private bool goalIsInGeneralDirection( Point curr, eDirections dir, Point goal )
	{
		int diff_column = goal.x - curr.x;
		int diff_row    = goal.y - curr.y;

		// note: north would be DECREASING in row, not increasing. Rows grow positive while going south!
		switch ( dir )
		{
			case eDirections.NORTH:
				return diff_row < 0 && diff_column == 0;
			case eDirections.NORTH_EAST:
				return diff_row < 0 && diff_column > 0;
			case eDirections.EAST:
				return diff_row == 0 && diff_column > 0;
			case eDirections.SOUTH_EAST:
				return diff_row > 0 && diff_column > 0;
			case eDirections.SOUTH:
				return diff_row > 0 && diff_column == 0;
			case eDirections.SOUTH_WEST:
				return diff_row > 0 && diff_column < 0;
			case eDirections.WEST:
				return diff_row == 0 && diff_column < 0;
			case eDirections.NORTH_WEST:
				return diff_row < 0 && diff_column < 0;
		}

		return false;
	}

	/// <summary>
	/// Get the Node, starting at the given position, in the given direction, at the given distance away.
	/// </summary>
	private PathfindingNode getNodeDist( int row, int column, eDirections direction, int dist , PathfindingNode[] pathfindingNodes)
	{
		PathfindingNode new_node = null;
		int new_row = row, new_column = column;

		switch ( direction )
		{
			case eDirections.NORTH:
				new_row -= dist;
				break;
			case eDirections.NORTH_EAST:
				new_row -= dist;
				new_column += dist;
				break;
			case eDirections.EAST:
				new_column += dist;
				break;
			case eDirections.SOUTH_EAST:
				new_row += dist;
				new_column += dist;
				break;
			case eDirections.SOUTH:
				new_row += dist;
				break;
			case eDirections.SOUTH_WEST:
				new_row += dist;
				new_column -= dist;
				break;
			case eDirections.WEST:
				new_column -= dist;
				break;
			case eDirections.NORTH_WEST:
				new_row -= dist;
				new_column -= dist;
				break;
		}

		// w/ the new coordinates, get the node
		if ( isInBounds( new_row, new_column ) )
		{
			new_node = pathfindingNodes[ this.rowColumnToIndex( new_row, new_column ) ];
		}

		return new_node;
	}

	// Reverse the goal
	public List< Point > reconstructPath( PathfindingNode goal, Point start )
	{
		List< Point > path = new List< Point >();
		PathfindingNode curr_node = goal;

		while ( curr_node.parent != null )
		{
			path.Add( curr_node.pos );
			curr_node = curr_node.parent;
		}

		// Push starting node on there too
		path.Add( start );

		path.Reverse();  // really wish I could have just push_front but NO!
		return path;
	}

	public List<Point> getPath(int start, int target)
	{
		return getPath(gridNodes[start].pos, gridNodes[target].pos);
	}


    private PathfindingNode[] InitPathFindingNodes()
    {
        int numBlocks = w * h;
        PathfindingNode[] pathfindingNodes = new PathfindingNode[numBlocks];
        for (int i = 0; i < numBlocks; i++)
        {
            int x = i % w;
            int y = i / w;
            pathfindingNodes[i] = new PathfindingNode();
            pathfindingNodes[i].pos = new Point(x, y);
        }
        return pathfindingNodes;
    }

    private void ResetPathFindingNodes(PathfindingNode[] pathfindingNodes)
    {
        int numBlocks = w * h;
        for (int i = 0; i < numBlocks; i++)
        {
            int x = i % w;
            int y = i / w;
            pathfindingNodes[i].Reset();
        }
    }

    public List< Point > getPath( Point start, Point goal )
	{
        //ResetPathFindingNodes();
        PathfindingNode[] pathfindingNodes = InitPathFindingNodes();

		PriorityQueue< PathfindingNode, float > open_set = new PriorityQueue< PathfindingNode, float>();

        PathfindingNode starting_node = pathfindingNodes[ pointToIndex( start ) ];
		starting_node.pos = start;
		starting_node.parent = null;
		starting_node.givenCost = 0;
		starting_node.finalCost = 0;
		starting_node.pathStatus = PathStatus.ON_OPEN;

		open_set.push( starting_node, 0 );

		while ( ! open_set.isEmpty() )
		{
			PathfindingNode curr_node = open_set.pop();
			PathfindingNode parent = curr_node.parent;
			Node jp_node = gridNodes[ pointToIndex( curr_node.pos ) ];    // get jump point info

			// Check if we've reached the goal
			if ( curr_node.pos.Equals( goal ) ) 
			{
				// end and return path
				return reconstructPath( curr_node, start );
			}

			// foreach direction from parent
			foreach ( eDirections dir in getAllValidDirections( curr_node ) )
			{
				PathfindingNode new_successor = null;
				int given_cost = 0;

				// goal is closer than wall distance or closer than or equal to jump point distnace
				if ( isCardinal( dir ) &&
				     goalIsInExactDirection( curr_node.pos, dir, goal ) && 
				     Point.diff( curr_node.pos, goal ) <= Math.Abs( jp_node.jpDistances[ (int) dir ] ) )
				{
					new_successor = pathfindingNodes[ pointToIndex( goal ) ];

					given_cost = curr_node.givenCost + Point.diff( curr_node.pos, goal );
				}
				// Goal is closer or equal in either row or column than wall or jump point distance
				else if ( isDiagonal( dir ) &&
				          goalIsInGeneralDirection( curr_node.pos, dir, goal ) && 
				          ( Math.Abs( goal.x - curr_node.pos.x ) <= Math.Abs( jp_node.jpDistances[ (int) dir ] ) ||
				            Math.Abs( goal.y - curr_node.pos.y ) <= Math.Abs( jp_node.jpDistances[ (int) dir ] ) ) )
				{
					// Create a target jump point
					int min_diff = Math.Min( Math.Abs( goal.x - curr_node.pos.x ), 
					                          Math.Abs( goal.y - curr_node.pos.y ) );

					new_successor = getNodeDist( 
						curr_node.pos.y, 
						curr_node.pos.x, 
						dir, 
						min_diff,
                        pathfindingNodes);

					given_cost = curr_node.givenCost + (int)( SQRT_2 * Point.diff( curr_node.pos, new_successor.pos ) );
				}
				else if ( jp_node.jpDistances[ (int) dir ] > 0 )
				{
					// Jump Point in this direction
					new_successor = getNodeDist( 
						curr_node.pos.y, 
						curr_node.pos.x, 
						dir, 
						jp_node.jpDistances[ (int) dir ],
                        pathfindingNodes);
					
					given_cost = Point.diff( curr_node.pos, new_successor.pos );

					if ( isDiagonal( dir ) )
					{
						given_cost = (int)( given_cost * SQRT_2 );
					}

					given_cost += curr_node.givenCost;
				}

				// Traditional A* from this point
				if ( new_successor != null )
				{
					if ( new_successor.pathStatus != PathStatus.ON_OPEN )
					{
						new_successor.parent = curr_node;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.x, new_successor.pos.y, goal.x, goal.y );
						new_successor.pathStatus = PathStatus.ON_OPEN;
						open_set.push( new_successor, new_successor.finalCost );
					}
					else if ( given_cost < new_successor.givenCost )
					{
						new_successor.parent = curr_node;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.x, new_successor.pos.y, goal.x, goal.y );
						new_successor.pathStatus = PathStatus.ON_OPEN;
						open_set.push( new_successor, new_successor.finalCost );
					}
				}
			}
		}

		return null;
	}
}
