using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Player Reference (auto-found by tag if left empty)")]
    public string playerTag = "Player";
    public CharacterController player;
    public MonoBehaviour playerMovementScript;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool hasCheckpoint = false;

    private const string KeyX = "Checkpoint_X";
    private const string KeyY = "Checkpoint_Y";
    private const string KeyZ = "Checkpoint_Z";
    private const string KeyRotY = "Checkpoint_RotY";
    private const string KeyScene = "Checkpoint_Scene";
    private const string KeyExists = "Checkpoint_Exists";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayerIfNeeded();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.GetComponent<CharacterController>();
        }
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        checkpointPosition = position;
        checkpointRotation = rotation;
        hasCheckpoint = true;

        PlayerPrefs.SetFloat(KeyX, position.x);
        PlayerPrefs.SetFloat(KeyY, position.y);
        PlayerPrefs.SetFloat(KeyZ, position.z);
        PlayerPrefs.SetFloat(KeyRotY, rotation.eulerAngles.y);
        PlayerPrefs.SetString(KeyScene, SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt(KeyExists, 1);
        PlayerPrefs.Save();

        Debug.Log("Checkpoint saved: " + position);
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(KeyExists, 0) == 1;
    }

    public static void ContinueGame()
    {
        if (!HasSavedGame())
        {
            Debug.LogWarning("No saved checkpoint to continue from.");
            return;
        }
        string sceneName = PlayerPrefs.GetString(KeyScene, SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(sceneName);

    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(KeyX);
        PlayerPrefs.DeleteKey(KeyY);
        PlayerPrefs.DeleteKey(KeyZ);
        PlayerPrefs.DeleteKey(KeyRotY);
        PlayerPrefs.DeleteKey(KeyScene);
        PlayerPrefs.DeleteKey(KeyExists);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = null;
        FindPlayerIfNeeded();

        if (HasSavedGame() && player != null)
        {
            LoadCheckpointFromPrefs();
            RespawnPlayer();
        }
    }

    private void LoadCheckpointFromPrefs()
    {
        checkpointPosition = new Vector3(
            PlayerPrefs.GetFloat(KeyX),
            PlayerPrefs.GetFloat(KeyY),
            PlayerPrefs.GetFloat(KeyZ));
        checkpointRotation = Quaternion.Euler(0f, PlayerPrefs.GetFloat(KeyRotY), 0f);
        hasCheckpoint = true;
    }

    public void RespawnPlayer()
    {
        if (!hasCheckpoint || player == null) return;

        player.enabled = false;
        player.transform.position = checkpointPosition;
        player.transform.rotation = checkpointRotation;
        player.enabled = true;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }
}

