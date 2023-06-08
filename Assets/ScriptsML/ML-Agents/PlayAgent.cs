using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayAgent : Agent
{
    // Field x : -10 ~ 1, z : -8 ~ 3
    #region Declaration Part

    public Transform Target;
    public List<Transform> Obstacles;
    public MLPlayerControllerNew PlayerController;
    public RayPerceptionSensorComponent3D rayPerceptionSensorComponent3D;
    public bool rw;
    public Vector3 vel, angle_vel;
    #endregion

    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        vel = this.GetComponent<Rigidbody>().velocity;
        angle_vel = this.GetComponent<Rigidbody>().angularVelocity;
    }

    #endregion

    #region Agent

    public override void Initialize()
    {
        MaxStep = 3000;
        rayPerceptionSensorComponent3D = GetComponent<RayPerceptionSensorComponent3D>();
    }

    public override void OnEpisodeBegin()
    {
        vel = angle_vel = Vector3.zero;
        //transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        ChangeTargetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(rw);
        sensor.AddObservation(transform.localRotation.y);
        //Target��ġ�� ���� �޾ƿͼ� ��ó�� ��ġ�� �ƴ°� �ƴѰ�?
        //sensor.AddObservation(Target.localPosition.x);
        //sensor.AddObservation(Target.localPosition.z);
        sensor.AddObservation(RayCastInfo(rayPerceptionSensorComponent3D));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions;

        // �̵��� ����
        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;

        if (!(RayCastInfo(rayPerceptionSensorComponent3D) == null))
        {
            if (CheckDistance(RayCastInfo(rayPerceptionSensorComponent3D)) < 3.0f|| CheckDistance(RayCastInfo(rayPerceptionSensorComponent3D)) > 15.0f)
            {
                AddReward(-1.0f / (float)MaxStep);
            }
            else
            {
                AddReward(+0.1f);
            }

            AddReward(+1.0f / (float)MaxStep);

            float angle_btw_v = CheckAngle(RayCastInfo(rayPerceptionSensorComponent3D));
            if (Mathf.Abs(angle_btw_v) <= 10)
            {
                AddReward(+0.3f);
                PlayerController.OnShoot();
            }
        }




        switch (action[0])
        {
            case 1: dir = transform.forward; break;
            case 2: break;
            //case 2: dir = -transform.forward; break;
        }

        switch (action[1])
        {
            case 1: rot = -transform.up; break;
            case 2: rot = transform.up; break;
        }

        transform.Rotate(rot, Time.deltaTime * 100.0f);
        //this.GetComponent<Rigidbody>().AddForce(dir * 2.0f, ForceMode.VelocityChange);
        //this.transform.TransformPoint((transform.position + dir) * Time.deltaTime);
        StartCoroutine(PlayerMoveML(dir));
        //transform.localPosition = transform.localPosition + (transform.localRotation * dir * 5.0f * Time.deltaTime);
        //this.GetComponent<Rigidbody>().velocity = dir * 2.0f;
        AddReward(-1.0f / (float)(MaxStep));

    }
    IEnumerator PlayerMoveML(Vector3 dir)
    {
        transform.localPosition = transform.localPosition + (transform.localRotation * dir * 7.0f * Time.deltaTime);
        yield return new WaitForEndOfFrame();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;
        //actionsOut.Clear();

        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            action[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            action[1] = 2;
        }
    }

    #endregion



    public void SetReward(bool re)
    {

        rw = re;
        if (re)
            SetReward(+1.0f);
        else
            AddReward(-1.0f / MaxStep);
    }


    private GameObject RayCastInfo(RayPerceptionSensorComponent3D rayComponent)
    {
        var rayOutputs = RayPerceptionSensor
                .Perceive(rayComponent.GetRayPerceptionInput())
                .RayOutputs;

        if (rayOutputs != null)
        {
            var lengthOfRayOutputs = RayPerceptionSensor
                    .Perceive(rayComponent.GetRayPerceptionInput())
                    .RayOutputs
                    .Length;

            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                if (goHit != null)
                {
                    // Found some of this code to Denormalized length
                    // calculation by looking trough the source code:
                    // RayPerceptionSensor.cs in Unity Github. (version 2.2.1)
                    var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                    var scaledRayLength = rayDirection.magnitude;
                    float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                    // Print info:
                    string dispStr;
                    dispStr = "__RayPerceptionSensor - HitInfo__:\r\n";
                    dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                    dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                    dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                    //Debug.Log(dispStr);


                    //// Ray -> Enemy
                    //// ���� True ��ȯ
                    //// Angle�� ���밪 10�̳� -> +1.0
                    if (goHit.tag == "Enemy")
                    {
                        //return CheckAngle(goHit);
                        return goHit;
                    }
                    else return null;
                    //return goHit;
                }
                else return null;
            }
        }
        return null;
    }

    public void ChangeTargetPosition()
    {
        float rangeX = 31.0f;
        float rangeY = -31.0f;
        float randX = Random.Range(rangeX, rangeY);
        float randZ = Random.Range(rangeX, rangeY);
        if (Mathf.Abs(randZ) <= 22 && Mathf.Abs(randZ) > 16)
        {
            do {
                randX = Random.Range(rangeX, rangeY);
            } while (Mathf.Abs(randX) <= 20.5 && Mathf.Abs(randX) > 4);
        }
        else if (Mathf.Abs(randZ) <= 16 && Mathf.Abs(randZ) > 11)
        {
            do{
                randX = Random.Range(rangeX, rangeY);
            } while (Mathf.Abs(randX) <= 20.5 && Mathf.Abs(randX) > 11.5);
        }
        else if (Mathf.Abs(randZ) <= 11 && Mathf.Abs(randZ) > 3)
        {
            do{
                randX = Random.Range(rangeX, rangeY);
            } while (Mathf.Abs(randX) <= 20.5 && Mathf.Abs(randX) > 15.5);
        }
        Target.transform.localPosition = new Vector3(randX, 1.0f, randZ);
    }

    public float CheckAngle(GameObject goHit)
    {
        Vector3 btwV = (goHit.transform.position - transform.position);
        Vector3 vvv = btwV - transform.forward;
        float ag = Vector3.SignedAngle(transform.forward, vvv, transform.up);

        return ag;
    }

    public float CheckDistance(GameObject enemy)
    {
        float dis = Vector3.Distance(enemy.transform.position, transform.position);
        return dis;
    }
}