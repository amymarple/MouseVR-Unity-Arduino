using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	[SerializeField]
	KeyCode reset;

	[SerializeField]
	KeyCode keyUp;

	[SerializeField]
	KeyCode keyDown;

	[SerializeField]
	KeyCode keyLeft;

	[SerializeField]
	KeyCode keyRight;

	CharacterController characterController;
	public float keyboardMovementScale = 3.0f;
	public bool playerIsInSafeZone;
	public int number_of_teleports = 0;
	public bool water_is_available = true;

	public GameObject trackball;
	public float forwardBallMovement, rotationBallMovement, strafeBallMovement;
	public bool running, rotation, strafing, backwards;
	public float forward_velocity_scale_factor = 10.16f;
	public float stop_speed_threshold = 0.1f;

	public float smoothed_Rx;

	public EventLogger eventLogger;
	public KeyCode saver;
	public float[] positionLogger;
	public float[] positionRecorder;
	public float[] rotationLogger;
	public float[] rotationRecorder;
	public float[] ballrotationRecorder;

	public bool save_on_quit = true;

	float last_save_time;

	public ArduinoInterface arduinoInterface;
	public Velocity velocity;
	

	void Start() {
		water_is_available = true;
		characterController = gameObject.GetComponent<CharacterController>();
		number_of_teleports = 0;
		positionLogger = new float[3];
		rotationLogger = new float[3];
		positionRecorder = new float[3];
		rotationRecorder = new float[3];
		last_save_time = Time.time;
	}

	(Vector3 movement_keyboard, float rotation) getKeyboardMovement()
    {
		Vector3 movement = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.UpArrow))
        {
			movement += new Vector3(0, 0, 1);
        }
		if (Input.GetKey(KeyCode.DownArrow))
		{
			movement += new Vector3(0, 0, -1);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			movement += new Vector3(-1, 0, 0);
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			movement += new Vector3(1, 0, 0);
		}

		return (movement, 0.0f);

	}

	void Update() {

		(Vector3 movement_keyboard, float keyboard_rotation) = getKeyboardMovement();

		float s = Time.deltaTime;
		float c = 2.0f * 3.141592654f / 360.0f * 4.0f;
		if (running)
		{
			forwardBallMovement = arduinoInterface.Rx * s * c;
        }
        else
        {
			forwardBallMovement = 0;

		}
		if (!backwards)
		{
			forwardBallMovement = Mathf.Max(0, forwardBallMovement);
		}
		if (strafing)
		{
			strafeBallMovement = arduinoInterface.Rz * s * c;
		}
		else
		{
			strafeBallMovement = 0;
		}
		rotationBallMovement = arduinoInterface.Ry*s;

		if (Input.GetKey(reset))
		{
			Vector3 rotationVector = new Vector3(0, 0, 0);
			Quaternion rotation = Quaternion.Euler(rotationVector);
			transform.position = new Vector3(-2, 1, 0);
			eventLogger.Add(new Event("RESET"));
		}


		Vector3 movementVector = gameObject.transform.forward * forwardBallMovement - gameObject.transform.right * strafeBallMovement;
		characterController.Move(movementVector * forward_velocity_scale_factor + movement_keyboard*Time.deltaTime* keyboardMovementScale);

		if (rotation)
		{
			if (Input.GetKey(reset))
			{
				Vector3 rotationVector = new Vector3(0, 0, 0);
				Quaternion rotation = Quaternion.Euler(rotationVector);
				transform.position = new Vector3(-2, 1, 0);
				eventLogger.Add(new Event("RESET"));
			}
			else if (rotationBallMovement != 0)
			{
				characterController.transform.Rotate(0, rotationBallMovement, 0, Space.World);
				rotationLogger[0] = ((float)transform.eulerAngles.x);
				rotationLogger[1] = ((float)transform.eulerAngles.y);
				rotationLogger[2] = ((float)transform.eulerAngles.z);
				rotationRecorder = (float[])rotationLogger.Clone();
				eventLogger.Add(new Event("Rotated", rotationRecorder));
			}
		}
		if (true) {
			float[] ballrotationRecorder = {arduinoInterface.Rx, arduinoInterface.Ry, arduinoInterface.Rz};
			eventLogger.Add(new Event("[Rx,Ry,Rz]", ballrotationRecorder));
		}



		smoothed_Rx = 0.1f * arduinoInterface.Rx + smoothed_Rx * 0.9f;

		if (Input.GetKeyDown(saver))
		{
			eventLogger.saveEvents();
		}

        if (Time.time - last_save_time >= 1 / 30.0f)
        {

			float[] position = { gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z };
            //eventLogger.Add(new Event("POSITION", position));
            last_save_time = Time.time;
        }

    }


	public void deliverWater()
	{
		arduinoInterface.deliverWater();
		Debug.Log("Delivered Water");
	}

	public void lick(int numberOfLicks)
    {

		 
		if (playerIsInSafeZone & smoothed_Rx < stop_speed_threshold &  water_is_available)
        {
			water_is_available = false;
			deliverWater();


		}
	}

	public void deliverAirpuff()
    {
		Debug.Log("DELIVERED AIR PUFF");
		eventLogger.Add(new Event("DELIVERED AIRPUFF"));
		arduinoInterface.deliverAirpuff();
	}
 
	public void teleport(Vector3 teleportVector)
    {
		characterController.Move(teleportVector);
		number_of_teleports += 1;
		water_is_available = true;
		Debug.Log("TELEPORT");
		eventLogger.Add(new Event("TELEPORT"));
	}

	public void OnApplicationQuit()
	{
		if (save_on_quit)
		{
			Debug.Log("SAVING RUN DATA.");
			eventLogger.saveEvents();
		}
	}
}
