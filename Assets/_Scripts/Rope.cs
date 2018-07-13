using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Vector2 _destiny;
	public float _speed = 0.5f; 
	public float _distance = 2f; 
	public GameObject _nodePrefab; 
	public GameObject _player;
	private GameObject _lastNode; 
	private bool _isDone = false;

    // Use this for initialization
    void Start()
    {
		_player = GameObject.FindGameObjectWithTag("Player");
		_lastNode = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
	        
		if((Vector2)transform.position != _destiny)
		{
			transform.position = Vector2.MoveTowards(transform.position, _destiny, _speed); 

			if(Vector2.Distance(_player.transform.position, _lastNode.transform.position ) > _distance)
			{
				CreateNode();
			}
		}
		else if(_isDone == false)
		{
			_lastNode.GetComponent<HingeJoint2D>().connectedBody = _player.GetComponent<Rigidbody2D>();
			_isDone = true;
		}

		if(_isDone == true && Input.GetMouseButtonUp(1))
		{
			_lastNode.GetComponent<HingeJoint2D>().connectedBody = _lastNode.GetComponent<Rigidbody2D>();
			Destroy(gameObject); 
		}

    }

	void CreateNode()
	{
		Vector2 pos2create = _player.transform.position - _lastNode.transform.position;
		pos2create.Normalize();
		pos2create *= _distance;
		pos2create += (Vector2)_lastNode.transform.position;

		GameObject go = Instantiate(_nodePrefab, pos2create, Quaternion.identity);
		go.transform.SetParent(transform);

		_lastNode.GetComponent<HingeJoint2D>().connectedBody = go.GetComponent<Rigidbody2D>();
		 _lastNode.GetComponent<HingeJoint2D>().anchor = new Vector2(0.0f, -1.10f);

		 _lastNode.GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0.0f, 1.1f);
		 _lastNode.GetComponent<HingeJoint2D>().useLimits = false;
		_lastNode.gameObject.AddComponent<BoxCollider2D>();
		_lastNode = go;
	}
}
