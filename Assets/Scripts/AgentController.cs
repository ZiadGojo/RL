using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;


public class AgentController : Agent
{
    public Transform[] spawnPoints;



    //but de rl c'est maximiser la rew
    //etat d'action set de tout les possible etat dans un env
    ///these de rew tout les buts peuvent expresser la max rew <summary>
    /// these de rew tout les buts peuvent expresser la max rew
    /// group etat dis con espa
    /// valeur de ex cum rew
    /// m a env p rew s v f m
    /// </summary>


//Pellet Variables
[SerializeField] private Transform target;
    public int pelletCount;
    public GameObject food;
    [SerializeField] private List<GameObject> spawnedPelletList = new List<GameObject>();

    //Agent Vairables

    [SerializeField] private float moveSpeed = 10f;
    private Rigidbody rb;

    //Env variables
    [SerializeField] private Transform environmentLocation;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }


    public override void OnEpisodeBegin()
    {
        //Agent
        transform.localPosition = new Vector3(2.71f, 0.3f, Random.Range(-29f, 25f));

        CreatePellet();

        //target.localPosition = new Vector3(Random.Range(-29f, 39f), 0.3f, Random.Range(-29f, 25f));
    }

    private void CreatePellet()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform randomSpawnPoint = spawnPoints[randomIndex];

        if (spawnedPelletList.Count != 0) {
            RemovePellet(spawnedPelletList);
        }
        for (int i = 0; i < pelletCount; i++)
        {

            int counter = 0;
            bool distancegood;
            bool alreaddec = false;
            //Spawning
            GameObject newPellet = Instantiate(food);
            //Make pellet the child of the env
            newPellet.transform.parent = environmentLocation;
            //Give random spawn location
            Vector3 pelletLocation = spawnPoints[randomIndex].localPosition;

            if (spawnedPelletList.Count != 0) { 
            for (int k = 0;k<spawnedPelletList.Count; k++)
                {
                    if (counter < 10)
                    {
                        distancegood = CheckOverlap(pelletLocation, spawnedPelletList[k].transform.localPosition, 5f);
                        if(distancegood == false)
                        {
                            randomIndex = Random.Range(0, spawnPoints.Length);
                            pelletLocation = spawnPoints[randomIndex].localPosition;
                        k--;
                            alreaddec = true;
                            Debug.Log("Too close to other pellet");
                        }
                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletList.Count;
                    }
                }
            }
            
            //Spawn in new location
            newPellet.transform.localPosition = pelletLocation;
            // Add to list 
            spawnedPelletList.Add(newPellet);
        }
    }

    private bool CheckOverlap(Vector3 ObjectAvoidOverlap, Vector3 alreadyexist, float mindisntance)
    {
        float DistanceBetweenObjects = Vector3.Distance(ObjectAvoidOverlap, alreadyexist);
        if(mindisntance <= DistanceBetweenObjects)
        {
            return true;
        }
        return false;
    }


    private void RemovePellet(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
        /*

        Vector3 velocity = new Vector3(moveX, 0f,moveZ);
        velocity = velocity.normalized * Time.deltaTime * moveSpeed; 

        transform.localPosition += velocity;

        */
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contrinuousActions = actionsOut.ContinuousActions;
        contrinuousActions[0] = Input.GetAxisRaw("Horizontal");
        contrinuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pellet") {
            //Remove from the list 
            spawnedPelletList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            if(spawnedPelletList.Count == 0)
            {
                RemovePellet(spawnedPelletList);
                AddReward(5f);

                EndEpisode();
            }
        }
        if (other.gameObject.tag == "Wall")
        {
            RemovePellet(spawnedPelletList);
            AddReward(-15f);
            EndEpisode();
        }
    }


}
