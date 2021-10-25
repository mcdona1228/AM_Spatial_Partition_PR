using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        public GameObject friendlyObj;
        public GameObject enemyObj;

        //Change materials to detect which enemy is the closest
        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        //To get a cleaner workspace, parent all soldiers to these empty gameobjects
        public Transform enemyParent;
        public Transform friendlyParent;

        //Store all soldiers in these lists
        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        //Save the closest enemies to easier change back its material
        List<Soldier> closestEnemies = new List<Soldier>();

        //Grid data
        float mapWidth = 50f;
        int cellSize = 10;

        //Number of soldiers on each team
        int numberOfSoldiers = 100;

        //The Spatial Partition grid
        Grid grid;

        bool useSpatialPartition = false;

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI buttonText;

        public Button add50;
        public Button add500;
        public Button add1000;
        public Button reset;

        public int test;

        public void onButtonPress()
        {
            if(buttonText.text == "Enable S.P")
            {
                useSpatialPartition = true;
                buttonText.text = "Disable S.P";
            }
            else
            {
                useSpatialPartition = false;
                buttonText.text = "Enable S.P";
            }
        }

        public void onAdd50()
        {
            numberOfSoldiers += 50;
            Start();
        }

        public void onAdd500()
        {
            numberOfSoldiers += 500;
            Start();
        }

        public void onAdd1000()
        {
            numberOfSoldiers += 1000;
            Start();
        }

        public void onReset()
        {
            SceneManager.LoadScene(0);
        }


        void Start()
        {
            //Create a new grid
            grid = new Grid((int)mapWidth, cellSize);

            //Add random enemies and friendly and store them in a list
            for (int i = 0; i < numberOfSoldiers; i++)
            {
                //Give the enemy a random position
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), Random.Range(0f, mapWidth), Random.Range(0f, mapWidth));

                //Create a new enemy
                GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the enemy to a list
                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));

                //Parent it
                newEnemy.transform.parent = enemyParent;


                //Give the friendly a random position
                randomPos = new Vector3(Random.Range(0f, mapWidth), Random.Range(0f, mapWidth), Random.Range(0f, mapWidth));

                //Create a new friendly
                GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the friendly to a list
                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));

                //Parent it 
                newFriendly.transform.parent = friendlyParent;
            }
        }


        void Update()
        {
            float timeBegin = Time.realtimeSinceStartup;

            //Move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            //Reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            //Reset the list with closest enemies
            closestEnemies.Clear();
            Soldier closestEnemy;

            //For each friendly, find the closest enemy and change its color and chase it
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                if (useSpatialPartition)
                {
                    //The fast version with spatial partition
                    closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);
                }

                else
                {
                    //The Slow version without spatial partition
                    closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);
                }

                //If we found an enemy
                if (closestEnemy != null)
                {
                    //Change material
                    closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;

                    closestEnemies.Add(closestEnemy);

                    //Move the friendly in the direction of the enemy
                    friendlySoldiers[i].Move(closestEnemy);
                }
            }

            timeText.text = ((Time.realtimeSinceStartup - timeBegin) * 1000f) + "ms";
        }


        //Find the closest enemy - slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop thorugh all enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }
    }
}