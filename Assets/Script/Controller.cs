using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
	const float diff = 0.01f;
	const int NumOfTask = 10;
	const float maxTime = 20f;
	public float power = 1000f;

	public float[] interArrivalTime = new float[NumOfTask];
	public float[] arrivalTime = new float[NumOfTask];
	public float[] serviceTime = new float[NumOfTask];
	public GameObject[] taskObj = new GameObject[NumOfTask];

	public int curTaskNum = 0;
	public int readyTaskNum = 0;
	public int completedNum = 0;
	public float curTime = 0f;
	public float startTime = 0f;
	public float stopTime = 0f;
	public float totalWatingTime = 0f;

	public Text clockText;
	public Text B_t;
	public Text Q_t;
	public Text ArrivalTimesText;
	public Text eventCalander;
	public Text eventCalander1;
	public Text numOfCompleted;
	public Text totalWatingText;
	public Text interArrivalText;
	public Text serviceText;
	public BoxCollider2D wall;

	public bool nextStep = false;

	public List<EventNode> eventTimeList = new List<EventNode>();
	public List<EventNode> calanderList = new List<EventNode> ();
	public List<EventNode> inQueueList = new List<EventNode> ();
	int EventNum = 0;

	public void nextStepBtn() {
		if (EventNum >= eventTimeList.Count)
			return;
		EventNode tNode = eventTimeList [EventNum++];

		Debug.Log (tNode.taskNum + ", " + tNode.time + ", " + tNode.type);
		clockText.text = tNode.time.ToString();
		switch (tNode.type) {
		case EventType.Dep:
			EventNode tnode = null;
			foreach (EventNode nptr in inQueueList) {
				if (nptr.taskNum == tNode.taskNum)
					tnode = nptr;
			}
			inQueueList.Remove (tnode);
			calanderList.Remove (tnode);
			if (inQueueList.Count > 0)
				completedNum++;
			wall.isTrigger = true; 
			taskObj [int.Parse (tNode.taskNum)].GetComponent<Rigidbody2D> ().AddForce (Vector2.right * power);
			Invoke ("closeWall", 0.3f);
	
			if (inQueueList.Count > 0) {
				totalWatingTime += tNode.time - arrivalTime [int.Parse (tNode.taskNum)+1];
				totalWatingText.text = totalWatingTime.ToString ();
			}
			break;
		case EventType.Arr:
			inQueueList.Add (tNode);
			taskObj [int.Parse(tNode.taskNum)].SetActive (true);
			Debug.Log ("InQueue Count : " + inQueueList.Count);
			if (inQueueList.Count == 1)
				completedNum++;
			break;
		case EventType.End:

			break;
		default:
			break;
		}
		numOfCompleted.text = completedNum.ToString();
		UpdatePanel ();
	}

	void UpdatePanel() { //B(T)
		if (inQueueList.Count > 0)
			B_t.text = "1";
		else
			B_t.text = "0";

		if (inQueueList.Count > 1)
			Q_t.text = (inQueueList.Count - 1).ToString();
		else
			Q_t.text = "0";

		string calanderStr = "";

		foreach (EventNode node in eventTimeList) { //event-calendar
			if (node.time > float.Parse (clockText.text)) {
				//calanderStr += "[" + node.taskNum.ToString () + ", " + node.time + ", ";

				switch (node.type) { 
				case EventType.Dep:
					calanderStr += "[" + (int.Parse(node.taskNum)+1).ToString () + ", " + node.time + ", Dep]\n";
					break;
				case EventType.Arr:
					calanderStr += "[" + (int.Parse(node.taskNum)+1).ToString () + ", " + node.time + ", Arr]\n";
					break;
				case EventType.End:
					calanderStr += "[" + node.taskNum.ToString () + ", " + node.time + ", End]\n";
					break;
				default:
					break;
				}
			}

		}
		eventCalander.text = calanderStr;

		if (inQueueList.Count <= 1) {
			ArrivalTimesText.text = "<empty>";
		} else {
			string arrivalTimes = "";
			for (int i = completedNum + 1; i < inQueueList.Count+completedNum; i++) {
				arrivalTimes = arrivalTime [i].ToString () + " " + arrivalTimes;
			}
			arrivalTimes = "(" + arrivalTimes + ")";
			ArrivalTimesText.text = arrivalTimes;
		}
	}



	void Start () {
		arrivalTime [0] = interArrivalTime [0];
		for (int i = 1; i < NumOfTask; i++) {
			arrivalTime [i] = arrivalTime [i - 1] + interArrivalTime [i];
		}

		//stopTime = arrivalTime [0];
		stopTime = arrivalTime[NumOfTask-1];

		for (int i = 0; i < NumOfTask; i++) {
			taskObj [i] = GameObject.Find ("Task_" + (i + 1).ToString ());
			taskObj [i].SetActive (false);
		}


		while (true) {
			if (curTaskNum >= NumOfTask)
				break;
			if (isEqual(stopTime,startTime + serviceTime [curTaskNum])) {
				AddNode ((curTaskNum).ToString(), stopTime, EventType.Dep);
				//AddNode (curTaskNum.ToString(), stopTime, EventType.End);

				//release cur task
				//catch next task
				curTaskNum++;
				startTime = stopTime;
			}
			if (isEqual(stopTime,arrivalTime [readyTaskNum])) {
				AddNode (readyTaskNum.ToString(), stopTime, EventType.Arr);
				readyTaskNum++;
				// pop Ready Queue
			}
			UpdateStopTime ();
			if (stopTime >= maxTime) {
				AddNode ("-", maxTime, EventType.End);
				break;
			}
		}

		for (int i = 0; i < NumOfTask; i++) {
			interArrivalText.text = interArrivalText.text + interArrivalTime[i].ToString() + ", ";
			serviceText.text = serviceText.text + serviceTime [i].ToString () + ", ";
		}
		interArrivalText.text += "...";
		serviceText.text += "...";
	}

	public void AddNode(string n, float t, EventType e) {
		EventNode tNode = new EventNode (n, t, e);
		eventTimeList.Add (tNode);
		calanderList.Add (tNode);
	}

	void UpdateStopTime () {
		if (readyTaskNum >= NumOfTask) {
			stopTime = startTime + serviceTime [curTaskNum];
		} else if (curTaskNum < readyTaskNum) {
			if (arrivalTime [readyTaskNum] < startTime + serviceTime [curTaskNum]) {
				stopTime = arrivalTime [readyTaskNum];
			} else {
				stopTime = startTime + serviceTime [curTaskNum];
			}
		} else {
			stopTime = arrivalTime [readyTaskNum];
		}
	}

	bool isEqual(float A, float B) {
		if (Mathf.Abs (A - B) < diff)
			return true;
		else
			return false;
	}

	public void closeWall() {
		wall.isTrigger = false;
	}
}


public enum EventType {Dep, Arr, End}

public class EventNode {
	
	public EventNode(string s, float t, EventType e) {
		taskNum = s;
		time = t;
		type = e;
		TaskObj = null;
	}

	public string taskNum;
	public float time;
	public EventType type;
	public GameObject TaskObj;
}