using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    WATER = 2,
    DRUG = 3,
    VIRUS = 4,
}

public class Level : MonoBehaviour
{
    // fields/variables you may adjust from Unity's interface
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float storey_height = 2.5f;   // height of walls
    public float virus_speed = 3.0f;     // virus velocity
    public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene
    public GameObject virus_prefab;
    public GameObject water_prefab;
    public GameObject house_prefab;
    public GameObject text_box;
    public GameObject scroll_bar;
    public GameObject play_again_button;
    public GameObject try_again_button;
    // public Text buttonText;


    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
    internal int num_virus_hit_concurrently = 0;            // how many viruses hit the player before washing them off
    internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
    internal float timestamp_virus_landed = float.MaxValue; // timestamp to check how many sec passed since the virus landed on player
    internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
    internal bool player_is_on_water = false;               // is player on water block
    internal bool player_entered_house = false;             // has player arrived in house?
    internal bool try_again = false;                        // Does player have to try again
    internal bool play_again = false;                     // Does player have to play again
    

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private int num_viruses = 0;             // number of viruses in the level
    private List<int[]> pos_viruses;         // stores their location in the grid
    private List<TileType>[,] global_grid;   // stores grid for try again
    private int global_wr;
    private int global_lr;
    private int global_wee;
    private int global_lee;
    private GameObject grave;


    // Audio Clips
    public AudioClip cough_clip;
    public AudioClip success_clip;
    public AudioClip dead_clip;
    public AudioClip water_splash_clip;
    public AudioClip drug_clip;

    public AudioSource source;

    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        // try_again_button.GetComponent<Button>().onClick.AddListener(delegate { replay_level(); });
        List<TileType>[,] grid = null;
        Init(grid);
    }

    private void Init(List<TileType>[,] grid)
    {
        // initialize internal/private variables
        bounds = GetComponent<Collider>().bounds; 
        timestamp_last_msg = 0.0f;
        function_calls = 0;
        num_viruses = 0;
        player_health = 1.0f;
        num_virus_hit_concurrently = 0;
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;       

        // Initialize AudioSource 
        source = GetComponent <AudioSource> ();

        // Disable button
        play_again_button.SetActive(false);
        try_again_button.SetActive(false);

        // Reloading previous level
        if(grid!=null) 
        {
            DrawDungeon (ref grid);
            return;
        }

        // initialize 2D grid
        grid = new List<TileType>[width, length];

        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // will place x viruses in the beginning (at least 1). x depends on the sise of the grid (the bigger, the more viruses)        
        num_viruses = width * length / 25 + 1; // at least one virus will be added
        pos_viruses = new List<int[]>();
        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the random initialization and try to re-solve the CSP
        bool success = false;        
        while (!success)
        {
            for (int v = 0; v < num_viruses; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a random location in the grid
                    int wr = Random.Range(1, width - 1);
                    int lr = Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                        pos_viruses.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG };
                            Shuffle<TileType>(ref candidate_assignments);

                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0; 
            }
        }
        global_grid = new List<TileType>[width, length];
        for(int i=0;i<width;++i)    
            for(int j=0;j<length;++j)
                global_grid[i, j] = new List<TileType> (grid[i, j]);
        
        DrawDungeon(ref grid);
    }

    public void replay_level()
    {
        Cursor.visible = false;
        DestroyObject(grave);
        System.Threading.Thread.Sleep(2000);
        Init(global_grid);
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWallsORWaterORDrug(ref List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10) ||
             (number_of_assigned_elements[(int)TileType.WATER] > (width + length) / 4) ||
             (number_of_assigned_elements[(int)TileType.DRUG] >= num_viruses / 2))
            return true;
        else
            return false;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    bool TooLongWall(ref List<TileType>[,] grid)
    {
        /*** implement the rest ! */

        // Check for horizontal walls
        for (int w = 1; w < width-1; w++) {
            for (int l = 1; l < length-3; l++) {
                if(grid[w, l].Count==1 && grid[w, l][0]==TileType.WALL 
                    && grid[w, l+1].Count==1 && grid[w, l+1][0]==TileType.WALL 
                        && grid[w, l+2].Count==1 && grid[w, l+2][0]==TileType.WALL)
                            return true;
            }
        }

        // Check for vertical walls
        for (int l = 1; l < length-1; l++) {
            for (int w = 1; w < width-3; w++) {
                if(grid[w, l].Count==1 && grid[w, l][0]==TileType.WALL 
                    && grid[w+1, l].Count==1 && grid[w+1, l][0]==TileType.WALL 
                        && grid[w+2, l].Count==1 && grid[w+2, l][0]==TileType.WALL)
                            return true;
            }
        }

        return false;
    }

    bool CheckWall(int x, int y, List<TileType>[,] grid)
    {
        // unassigned
        if(grid[x, y].Count!=1)
            return true;
        
        // if assigned check wall
        if(grid[x, y].Count==1){
            // Check for exterior wall
            if(x==0 || y==0 || x==width-1 || y==length-1 || grid[x, y][0]==TileType.WALL){
                return true; 
            }
            return false;
        }
        return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus 
    // adjacency means left, right, top, bottom, and *diagonal* blocks
    bool NoWallsCloseToVirus(ref List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        List<int> posX = new List<int> {-1, -1,  0,  1, 1, 1, 0, -1};
        List<int> posY = new List<int> { 0, -1, -1, -1, 0, 1, 1,  1};

        int x, y;
        bool res;

        for (int w = 1; w < width-1; w++)
        {
            for (int l = 1; l < length-1; l++) 
            {
                if(grid[w, l][0]==TileType.VIRUS)
                {
                    res = false;

                    for(int pos = 0; pos<8; ++pos)
                    {
                        x = w + posX[pos];
                        y = l + posY[pos];
                        // If all neighbors are assigned and none of them are wall, then break function
                        res = res || CheckWall(x, y, grid);
                    }  
                    if(!res){
                        // Debug.Log("No Walls close to Virus!!!");   
                        return true;
                    }
                }
            }
        }

        // Debug.Log("There is a Wall/Unassigned cell close to Virus!!!");
        return false;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooManyInteriorWallsORWaterORDrug(ref grid) && !TooLongWall(ref grid) && !NoWallsCloseToVirus(ref grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking 
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 100000)       
            return false;

        // we are done!
        if (unassigned.Count == 0 )
            return true;

        /*** implement the rest ! */
        
        // get the cell to be filled
        int[] cell_pos = unassigned[unassigned.Count-1];
        int w = cell_pos[0], l = cell_pos[1];
        List<TileType> tiles = grid[w, l];
        foreach (var tile in tiles) {
            if(CheckConsistency(grid, cell_pos, tile))
            {
                // update grid and unassigned
                grid[w, l] = new List<TileType> {tile};
                unassigned.RemoveAt(unassigned.Count-1);

                if(BackTrackingSearch(grid, unassigned))
                    return true;

                // Backtrack
                grid[w, l] = tiles;
                unassigned.Add(cell_pos);
            }
        }

        // None of possible
        return false;
    }

    bool checkValidPath(List<TileType>[,] grid, int w, int l, int wee, int lee) 
    {
        // for BFS
        List<int[]> queue = new List<int[]>();
        int[] pos = new int[2]{w, l}; 
        queue.Add(pos);

        // To keep track of visited vertices
        Hashtable visited = new Hashtable();
        visited.Add(getPositionHash(pos), true);

        int[] dirX = new int[4]{-1, 1, 0, 0};
        int[] dirY = new int[4]{0, 0, -1, 1};

        while (queue.Count>0)
        {
            pos = queue[0]; queue.RemoveAt(0);
            if (pos[0]==wee && pos[1]==lee) // Valid Path
                return true;
            
            for(int i=0;i<4;++i)
            {
                int[] new_pos = new int[2];
                new_pos[0] = pos[0] + dirX[i];  
                new_pos[1] =  pos[1] + dirY[i];
                if(new_pos[0]==wee && new_pos[1]==lee) // Valid Path
                    return true;
                if(0<=new_pos[0] && new_pos[0]<width && 0<=new_pos[1] && new_pos[1]<length 
                    && !visited.ContainsKey(getPositionHash(new_pos)) 
                        && grid[new_pos[0], new_pos[1]][0]!=TileType.WALL)
                        {
                            visited.Add(getPositionHash(new_pos), true);
                            queue.Add(new_pos);
                        }
            }
        }

        // No Valid Path
        return false;
    }

    // Returns the Hash value of a grid
    string getPositionHash(int [] pos)
    {   
        string hash = "";
        foreach(var i in pos)
            hash += i.ToString() + ",";
        return hash;
    }

    string getGridHash(List<TileType>[,] grid)
    {
        string hash = "";
        for(int i=0;i<width;++i)
            for(int j=0;j<length;++j)
                hash += grid[i, j][0].ToString() + ",";
        return hash;
    }

    bool checkBounds(int[] pos)
    {
        return 0<=pos[0] && pos[0]<width && 0<=pos[1] && pos[1]<length;
    }

    bool isOuterWall(int[] pos)
    {
        return pos[0]==0 || pos[0]==width-1 || pos[1]==0 || pos[1]==length-1;
    }


    List<TileType>[,] removeWalls(List<TileType>[,] grid, int wr, int lr, int wee, int lee)
    {
        
        List<TileType>[,] temp = new List<TileType>[width, length];

        // BFS-1 : Starts exploring walls around the goal
        int[] pos = new int[2] {wee, lee};

        // For exploring the neighbors in BFS
        int[] dirX = new int[4]{-1, 1, 0, 0};
        int[] dirY = new int[4]{0, 0, -1, 1};

        int startX = -1, startY = -1;
        
        // Find the wall adjacent to goal
        for(int i=0;i<4;++i)
        {
            int[] new_pos = new int[2];
            new_pos[0] = pos[0] + dirX[i];  
            new_pos[1] =  pos[1] + dirY[i];
            if (checkBounds(new_pos) && grid[new_pos[0], new_pos[1]][0]==TileType.WALL) {
                startX = new_pos[0];
                startY = new_pos[1];
                break;
            }
        }

        List<int[]> q = new List<int[]>();
        pos[0] = startX; pos[1] = startY;
        q.Add(pos);

        Hashtable vis = new Hashtable();
        vis.Add(getPositionHash(pos), true);

        for(int i=0;i<width;++i)
            for(int j=0;j<length;++j)
                temp[i, j] = new List<TileType>(grid[i, j]);

        while(q.Count>0)
        {
            pos = q[0];
            q.RemoveAt(0);

            if(!isOuterWall(pos))
                temp[pos[0], pos[1]][0] = TileType.FLOOR;

            if(checkValidPath(temp, wr, lr, wee, lee))
            {
                Debug.Log("Found my Path through BFS1");
                printGrid(temp, wr, lr, wee, lee);
                return temp;
            }

            for(int k=0;k<4;++k)
            {
                int[] new_pos = new int[2];
                new_pos[0] = pos[0] + dirX[k];  
                new_pos[1] = pos[1] + dirY[k];
                if (checkBounds(new_pos) && !vis.ContainsKey(getPositionHash(new_pos)) && grid[new_pos[0], new_pos[1]][0]==TileType.WALL)
                {
                    vis.Add(getPositionHash(new_pos), true);
                    q.Add(new_pos);
                }
            }
        }

        Debug.Log("No updates from BFS-1. Proceeding to BFS-2");

        // BFS-2: Exploring the walls around the player start position
        pos[0] = wr; pos[1] = lr;
        startX = -1; startY = -1;
        for(int i=0;i<4;++i)
        {
            int[] new_pos = new int[2];
            new_pos[0] = pos[0] + dirX[i];  
            new_pos[1] =  pos[1] + dirY[i];
            if (checkBounds(new_pos) && grid[new_pos[0], new_pos[1]][0]==TileType.WALL) {
                startX = new_pos[0];
                startY = new_pos[1];
                break;
            }
        }
         
        // Check the neighbors of neighbors
        if(startX==-1){
            int[] orig_pos = new int[2];
            orig_pos[0] = pos[0]; orig_pos[1]= pos[1];

            for(int k=0;k<4;++k)
            {
                pos[0] = orig_pos[0] + dirX[k];
                pos[1] = orig_pos[1] + dirY[k];

                for(int i=0;i<4;++i)
                {
                    int[] new_pos = new int[2];
                    new_pos[0] = pos[0] + dirX[i];  
                    new_pos[1] =  pos[1] + dirY[i];
                    if (checkBounds(new_pos) && grid[new_pos[0], new_pos[1]][0]==TileType.WALL) {
                        startX = new_pos[0];
                        startY = new_pos[1];
                        break;
                    }
                }
            }
        }

        q = new List<int[]>();
        pos[0] = startX; pos[1] = startY;
        q.Add(pos);

        vis = new Hashtable();
        vis.Add(getPositionHash(pos), true);

        for(int i=0;i<width;++i)
            for(int j=0;j<length;++j)
                temp[i, j] = new List<TileType>(grid[i, j]);
        
        while(q.Count>0)
        {
            pos = q[0];
            q.RemoveAt(0);

            if(!isOuterWall(pos))
                temp[pos[0], pos[1]][0] = TileType.FLOOR;

            if(checkValidPath(temp, wr, lr, wee, lee))
            {
                Debug.Log("Found my Path through BFS2");
                printGrid(temp, wr, lr, wee, lee);
                return temp;
            }

            for(int k=0;k<4;++k)
            {
                int[] new_pos = new int[2];
                new_pos[0] = pos[0] + dirX[k];  
                new_pos[1] = pos[1] + dirY[k];
                if (checkBounds(new_pos) && !vis.ContainsKey(getPositionHash(new_pos)) && grid[new_pos[0], new_pos[1]][0]==TileType.WALL)
                {
                    vis.Add(getPositionHash(new_pos), true);
                    q.Add(new_pos);
                }
            }
        }

        Debug.Log("BFS-2 done!! No updates :(");

        

        // Worst case return same grid
        return grid;
    }

    void printGrid(List<TileType>[,] new_solution, int wr, int lr, int wee, int lee)
    {
        string debug = "";
        for(int i=0;i<width;++i){
            for(int j=0;j<length;++j){
                if(i==wr && j==lr){
                    debug += "S ";
                    continue;
                }
                if(i==wee && j==lee){
                    debug += "E ";
                    continue;
                }

                if(new_solution[i, j][0]==TileType.WALL)
                    debug += "W ";
                if(new_solution[i, j][0]==TileType.DRUG)
                    debug += "D ";
                if(new_solution[i, j][0]==TileType.VIRUS)
                    debug += "V ";
                if(new_solution[i, j][0]==TileType.WATER)
                    debug += "A ";
                if(new_solution[i, j][0]==TileType.FLOOR)
                    debug += "F ";
            }
            debug += "\n";
        }   
        Debug.Log(debug);
    }

    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    void DrawDungeon(ref List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // if(wr==-1 && lr==-1)
        // {
            // place character at random position (wr, lr) in terms of grid coordinates (integers)
            // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
            int wr = 0;
            int lr = 0;
            while (true) // try until a valid position is sampled
            {
                wr = Random.Range(1, width - 1);
                lr = Random.Range(1, length - 1);

                if (solution[wr, lr][0] == TileType.FLOOR)
                {
                    float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                    float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                    fps_player_obj = Instantiate(fps_prefab);
                    fps_player_obj.name = "PLAYER";
                    // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                    fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f); 
                    break;
                }
            }
            // global_wr = wr;
            // global_lr = lr;
        // }
        // else
        // {
        //     float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
        //     float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
        //     fps_player_obj = Instantiate(fps_prefab);
        //     fps_player_obj.name = "PLAYER";
        //     // character is placed above the level so that in the beginning, he appears to fall down onto the maze
        //     fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f); 
        // }


        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
        // if(wee==-1 && lee==-1)
        // {
            int wee = -1, lee = -1;
            int max_dist = -1;
            
            while (true) // try until a valid position is sampled
            {
                if (wee != -1)
                    break;
                for (int we = 0; we < width; we++)
                {
                    for (int le = 0; le < length; le++)
                    {
                        // skip corners
                        if (we == 0 && le == 0)
                            continue;
                        if (we == 0 && le == length - 1)
                            continue;
                        if (we == width - 1 && le == 0)
                            continue;
                        if (we == width - 1 && le == length - 1)
                            continue;

                        if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                        {
                            // randomize selection
                            if (Random.Range(0.0f, 1.0f) < 0.1f)
                            {
                                int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                                if (dist > max_dist) // must be placed far away from the player
                                {
                                    wee = we;
                                    lee = le;
                                    max_dist = dist;
                                }
                            }
                        }
                    }
                }
            }
        //     global_wee = wee;
        //     global_lee = lee;
        // }
        

        // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
        // implement an algorithm that checks whether
        // all paths between the player at (wr,lr) and the exit (wee, lee)
        // are blocked by walls. i.e., there's no way to get to the exit!
        // if this is the case, you must guarantee that there is at least 
        // one accessible path (any path) from the initial player position to the exit
        // by removing a few wall blocks (removing all of them is not acceptable!)
        // this is done as a post-processing step after the CSP solution.
        // It might be case that some constraints might be violated by this
        // post-processing step - this is OK.
        
        /*** implement what is described above ! */

        if(!checkValidPath(solution, wr, lr, wee, lee))
        {
            Debug.Log("No Valid Path!!");

            Debug.Log("Before removing walls\n");
            printGrid(solution, wr, lr, wee, lee);

            // Use BFS with backtracking to remove minimum walls  
            List<TileType>[,] new_solution = new List<TileType>[width, length];
            for(int i=0;i<width;++i)
                for(int j=0;j<length;++j)
                    new_solution[i, j] = new List<TileType>(solution[i, j]);
            
            new_solution = removeWalls(new_solution, wr, lr, wee, lee);

            Debug.Log("Found a new solution!\n");
            Debug.Log("Valid path for new_soln = " + checkValidPath(new_solution, wr, lr, wee, lee));
            Debug.Log("Start: (" + wr.ToString() + "," + lr.ToString() + ")\n");
            Debug.Log("End: (" + wee.ToString() + "," + lee.ToString() + ")\n");
            printGrid(new_solution, wr, lr, wee, lee);

            
            // Update the solution now 
            for(int i=0;i<width;++i)
                for(int j=0;j<length;++j)
                    solution[i, j] = new List<TileType>(new_solution[i, j]);
            
            Debug.Log("Updated the solution grid, set to play!!!!\n");
           
        }
        else 
        {
            Debug.Log("Valid Path already!!");
        }
        


        // the rest of the code creates the scenery based on the grid state 
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                //Debug.Log(w + " " + l + " " + h);
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.8f, 0.8f);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    // virus.constraints = RigidbodyConstraints.FreezePositionY;
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    virus.transform.position = new Vector3(virus.transform.position.x, 1.0f, virus.transform.position.z);

                    //GameObject virus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //virus.GetComponent<Renderer>().material.color = new Color(0.5f, 0.0f, 0.0f);
                    //virus.name = "ENEMY";
                    //virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    //virus.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    //virus.AddComponent<BoxCollider>();
                    //virus.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.2f, 1.2f);
                    //virus.AddComponent<Rigidbody>();
                    //virus.GetComponent<Rigidbody>().useGravity = false;

                    virus.AddComponent<Virus>();
                    virus.GetComponent<Rigidbody>().mass = 10000;
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.name = "DRUG";
                    capsule.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    capsule.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    capsule.GetComponent<Renderer>().material.color = Color.green;
                    capsule.AddComponent<Drug>();
                }
                else if (solution[w, l][0] == TileType.WATER)
                {
                    GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "WATER";
                    water.transform.localScale = new Vector3(0.5f * bounds.size[0] / (float)width, 1.0f, 0.5f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WATER_BOX";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = Color.grey;
                    cube.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                    cube.GetComponent<BoxCollider>().isTrigger = true;
                    cube.AddComponent<Water>();
                }
            }
        }
    }


    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION JUST TO ADD SOUNDS ***
    // YOU MAY CHOOSE ANY SHORT SOUNDS (<2 sec) YOU WANT FOR A VIRUS HIT, A VIRUS INFECTION,
    // GETTING INTO THE WATER, AND REACHING THE EXIT
    // note: you may also change other scripts/functions to add sound functionality,
    // along with the functionality for the starting the level, or repeating it
    void Update()
    {
        if (player_health < 0.001f) // the player dies here
        {
            text_box.GetComponent<Text>().text = "Failed!";
            source.PlayOneShot(dead_clip);

            if (fps_player_obj != null)
            {
                grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grave.name = "GRAVE";
                grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
                grave.transform.position = fps_player_obj.transform.position;
                grave.GetComponent<Renderer>().material.color = Color.black;
                DestroyObject(fps_player_obj);
                System.Threading.Thread.Sleep(2000);
                Cursor.visible = true; 
                try_again_button.SetActive(true);
            }
            return;
        }
        if (player_entered_house) // the player suceeds here, variable manipulated by House.cs
        {   
            // source.PlayOneShot(success_clip);
            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Washed it off at home! Success!!!";
            else
                text_box.GetComponent<Text>().text = "Success!!!";
                
            DestroyObject(fps_player_obj);
            System.Threading.Thread.Sleep(1500);
            Cursor.visible = true; 
            play_again_button.SetActive(true);
            return;
        }

        if (Time.time - timestamp_last_msg > 7.0f) // renew the msg by restating the initial goal
        {
            text_box.GetComponent<Text>().text = "Find your home!";            
        }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        if (virus_landed_on_player_recently)
        {
            float time_since_virus_landed = Time.time - timestamp_virus_landed;
            if (time_since_virus_landed > 5.0f)
            {
                player_health -= Random.Range(0.25f, 0.5f) * (float)num_virus_hit_concurrently;
                player_health = Mathf.Max(player_health, 0.0f);
                if (num_virus_hit_concurrently > 1)
                    text_box.GetComponent<Text>().text = "Ouch! Infected by " + num_virus_hit_concurrently + " viruses";
                else
                    text_box.GetComponent<Text>().text = "Ouch! Infected by a virus";
                timestamp_last_msg = Time.time;
                timestamp_virus_landed = float.MaxValue;
                virus_landed_on_player_recently = false;
                num_virus_hit_concurrently = 0;
            }
            else
            {
                if (num_virus_hit_concurrently == 1)
                    text_box.GetComponent<Text>().text = "A virus landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
                else
                    text_box.GetComponent<Text>().text = num_virus_hit_concurrently + " viruses landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
            }
        }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
            if (player_health < 0.999f || virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! New drug helped!";
            else
                text_box.GetComponent<Text>().text = "No drug was needed!";
            timestamp_last_msg = Time.time;
            player_health += Random.Range(0.25f, 0.75f);
            player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // splashed on water  (boolean variable is manipulated by Water.cs)
        if (player_is_on_water)
        {
            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! Washed it off!";
            timestamp_last_msg = Time.time;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        scroll_bar.GetComponent<Scrollbar>().size = player_health;
        if (player_health < 0.5f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }

        /*** implement the rest ! */
    }
}

   


    