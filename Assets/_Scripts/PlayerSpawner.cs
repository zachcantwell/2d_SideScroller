using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _playerSpawnPoints;
	private GameObject _player; 
	private static int _activeSpawnLocation = 0;
    private static PlayerSpawner _playerSpawnerInstance; 
    // Use this for initialization

    private void Awake()
    {
        if(_playerSpawnerInstance != null && _playerSpawnerInstance != this)
        {                                         
            Destroy(this.gameObject);
        }
        else
        {
            _playerSpawnerInstance = this; 
        }
        DontDestroyOnLoad(_playerSpawnerInstance);
    }

    void Start()
    {
        if (_playerSpawnPoints == null)
        {
            Debug.LogWarning("Player Spawn Points not found");
        }
        else
        {
			_activeSpawnLocation = 0;
        }
    }

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

    private void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
		if(aScene.isLoaded)
		{
			_player = GameObject.FindGameObjectWithTag("Player");

			if(!_player)
			{
				Debug.LogWarning("Player not found");
			}
			else
			{
				_player.transform.position = _playerSpawnPoints[_activeSpawnLocation].transform.position 
											  + Vector3.up + Vector3.right/2;
			}
		}
    }

    private void Update()
    {
        SetActiveSpawner();
    }

    private void SetActiveSpawner()
    {
        if(_activeSpawnLocation < _playerSpawnPoints.Length - 1)
        {
            if(_player != null)
            {
                Vector3 spawnPos = _playerSpawnPoints[_activeSpawnLocation + 1].transform.position; 
                if(_player.transform.position.y >= spawnPos.y)
                {
                    //TODO: problem with this is that spawners will only work from left to right in the level.
                    // If there are spawners going from right to left in the level, they wont trigger. 
                    if(_player.transform.position.x >= spawnPos.x - 0.5f && _player.transform.position.x <= spawnPos.x + 0.5f)
                    {
                        _activeSpawnLocation++;
                    }
                }
            }

        }
    }
}
