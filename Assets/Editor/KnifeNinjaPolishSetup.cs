using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[InitializeOnLoad]
public static class KnifeNinjaPolishSetup
{
    private const string ScenePath = "Assets/Scenes/Level1.unity";
    private const string BackupScenePath = "Assets/Scenes/Level1_Original.unity";
    private const string MarkerPath = "Assets/KNIFE_NINJA_POLISHED.txt";
    private const string ArtFolder = "Assets/Resources/Level1Art";
    private const string AudioFolder = "Assets/Resources/Audio";
    private const string TileFolder = "Assets/Tiles/Polished";
    private const string AnimationFolder = "Assets/Animation";

    static KnifeNinjaPolishSetup()
    {
        EditorApplication.delayCall += AutoBuildOnce;
    }

    private static void AutoBuildOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (File.Exists(MarkerPath))
            return;

        BuildPolishedLevel1();
    }

    [MenuItem("Tools/Knife Ninja/Build Polished Level 1")]
    public static void BuildPolishedLevel1()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("Knife Ninja: Level1.unity was not found.");
            return;
        }

        try
        {
            EnsureFolders();
            ConfigureImportedArt();
            ConfigureAttackSprites();

            AssetDatabase.Refresh();

            Dictionary<string, Sprite> sprites = LoadArtSprites();
            if (!sprites.ContainsKey("tile_top") ||
                !sprites.ContainsKey("tile_fill") ||
                !sprites.ContainsKey("background"))
            {
                Debug.LogError("Knife Ninja: generated Level 1 art could not be imported.");
                return;
            }

            EnsureAnimationSetup();
            UpdateKnifePrefab(sprites);

            if (!File.Exists(BackupScenePath))
                AssetDatabase.CopyAsset(ScenePath, BackupScenePath);

            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single
            );

            BuildScene(scene, sprites);
            EnsureBuildSettings();

            File.WriteAllText(
                MarkerPath,
                "Knife Ninja polished Level 1 generated automatically.\n" +
                "Use Tools > Knife Ninja > Build Polished Level 1 to rebuild.\n"
            );

            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                "Knife Ninja: polished Level 1 created. " +
                "Original scene backed up as Level1_Original.unity."
            );
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError(
                "Knife Ninja polish setup failed. " +
                "Your original Level1 scene was left backed up when possible."
            );
        }
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(ArtFolder);
        Directory.CreateDirectory(AudioFolder);
        Directory.CreateDirectory(TileFolder);
        Directory.CreateDirectory(AnimationFolder);
        Directory.CreateDirectory("Assets/Sprites/Ninja/Attack");
        AssetDatabase.Refresh();
    }

    private static void ConfigureImportedArt()
    {
        foreach (string file in Directory.GetFiles(ArtFolder, "*.png"))
        {
            string path = file.Replace("\\", "/");
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit =
                Path.GetFileNameWithoutExtension(path) == "background"
                    ? 32f
                    : 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void ConfigureAttackSprites()
    {
        const string folder = "Assets/Sprites/Ninja/Attack";

        foreach (string file in Directory.GetFiles(folder, "*.png"))
        {
            string path = file.Replace("\\", "/");
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 80f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Dictionary<string, Sprite> LoadArtSprites()
    {
        Dictionary<string, Sprite> result =
            new Dictionary<string, Sprite>();

        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { ArtFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
                result[Path.GetFileNameWithoutExtension(path)] = sprite;
        }

        return result;
    }

    private static void EnsureAnimationSetup()
    {
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Sprites/Ninja/Idle/NinjaIdle.anim"
        );

        AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/NinjaRun.anim"
        );

        AnimationClip jump = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Sprites/Ninja/Jump/NinjaJump.anim"
        );

        if (idle != null)
            RetimeSpriteClip(idle, 10f, true);

        if (run != null)
            RetimeSpriteClip(run, 14f, true);

        if (jump != null)
            RetimeSpriteClip(jump, 16f, false);

        string throwClipPath = AnimationFolder + "/NinjaThrow.anim";
        AnimationClip throwClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            throwClipPath
        );

        if (throwClip != null)
            AssetDatabase.DeleteAsset(throwClipPath);

        string[] attackGuids = AssetDatabase.FindAssets(
            "t:Sprite",
            new[] { "Assets/Sprites/Ninja/Attack" }
        );

        List<Sprite> attackSprites = attackGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p, StringComparer.Ordinal)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
            .Where(s => s != null)
            .ToList();

        if (attackSprites.Count == 0)
            return;

        throwClip = new AnimationClip
        {
            name = "NinjaThrow",
            frameRate = 24f
        };

        EditorCurveBinding binding = new EditorCurveBinding
        {
            path = "",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keys =
            new ObjectReferenceKeyframe[attackSprites.Count];

        for (int i = 0; i < attackSprites.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / 24f,
                value = attackSprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(
            throwClip,
            binding,
            keys
        );

        AnimationClipSettings throwSettings =
            AnimationUtility.GetAnimationClipSettings(throwClip);
        throwSettings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(
            throwClip,
            throwSettings
        );

        AssetDatabase.CreateAsset(throwClip, throwClipPath);

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Sprites/Ninja/Idle/player.controller"
            );

        if (controller == null)
            return;

        EnsureParameter(
            controller,
            "Throw",
            AnimatorControllerParameterType.Trigger
        );

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;

        AnimatorState idleState = FindState(stateMachine, "NinjaIdle");
        AnimatorState runState = FindState(stateMachine, "NinjaRun");
        AnimatorState jumpState = FindState(stateMachine, "NinjaJump");
        AnimatorState throwState = FindState(stateMachine, "NinjaThrow");

        foreach (ChildAnimatorState child in stateMachine.states)
        {
            if (child.state == null)
                continue;

            foreach (AnimatorStateTransition transition in child.state.transitions)
            {
                transition.duration = 0.06f;
                transition.hasFixedDuration = true;
            }
        }

        if (throwState == null)
        {
            throwState = stateMachine.AddState(
                "NinjaThrow",
                new Vector3(520f, 10f, 0f)
            );
        }

        throwState.motion = throwClip;

        foreach (AnimatorStateTransition transition in
                 stateMachine.anyStateTransitions.ToArray())
        {
            if (transition.destinationState == throwState)
                stateMachine.RemoveAnyStateTransition(transition);
        }

        foreach (AnimatorStateTransition transition in
                 throwState.transitions.ToArray())
        {
            throwState.RemoveTransition(transition);
        }

        AnimatorStateTransition toThrow =
            stateMachine.AddAnyStateTransition(throwState);
        toThrow.hasExitTime = false;
        toThrow.duration = 0.04f;
        toThrow.canTransitionToSelf = false;
        toThrow.AddCondition(
            AnimatorConditionMode.If,
            0f,
            "Throw"
        );

        if (idleState != null)
        {
            AnimatorStateTransition t = throwState.AddTransition(idleState);
            t.hasExitTime = true;
            t.exitTime = 0.9f;
            t.duration = 0.06f;
            t.AddCondition(
                AnimatorConditionMode.If,
                0f,
                "IsGrounded"
            );
            t.AddCondition(
                AnimatorConditionMode.Less,
                0.1f,
                "Speed"
            );
        }

        if (runState != null)
        {
            AnimatorStateTransition t = throwState.AddTransition(runState);
            t.hasExitTime = true;
            t.exitTime = 0.9f;
            t.duration = 0.06f;
            t.AddCondition(
                AnimatorConditionMode.If,
                0f,
                "IsGrounded"
            );
            t.AddCondition(
                AnimatorConditionMode.Greater,
                0.1f,
                "Speed"
            );
        }

        if (jumpState != null)
        {
            AnimatorStateTransition t = throwState.AddTransition(jumpState);
            t.hasExitTime = true;
            t.exitTime = 0.9f;
            t.duration = 0.06f;
            t.AddCondition(
                AnimatorConditionMode.IfNot,
                0f,
                "IsGrounded"
            );
        }

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(throwClip);
        AssetDatabase.SaveAssets();
    }

    private static void RetimeSpriteClip(
        AnimationClip clip,
        float fps,
        bool loop
    )
    {
        EditorCurveBinding[] bindings =
            AnimationUtility.GetObjectReferenceCurveBindings(clip);

        EditorCurveBinding spriteBinding = bindings.FirstOrDefault(
            b => b.propertyName == "m_Sprite"
        );

        ObjectReferenceKeyframe[] oldKeys =
            AnimationUtility.GetObjectReferenceCurve(
                clip,
                spriteBinding
            );

        if (oldKeys == null || oldKeys.Length == 0)
            return;

        ObjectReferenceKeyframe[] keys =
            new ObjectReferenceKeyframe[oldKeys.Length];

        for (int i = 0; i < oldKeys.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = oldKeys[i].value
            };
        }

        clip.frameRate = fps;

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            spriteBinding,
            keys
        );

        AnimationClipSettings settings =
            AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AnimationUtility.SetAnimationEvents(
            clip,
            Array.Empty<AnimationEvent>()
        );

        EditorUtility.SetDirty(clip);
    }

    private static AnimatorState FindState(
        AnimatorStateMachine machine,
        string name
    )
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state != null && child.state.name == name)
                return child.state;
        }

        return null;
    }

    private static void EnsureParameter(
        AnimatorController controller,
        string name,
        AnimatorControllerParameterType type
    )
    {
        if (controller.parameters.Any(p => p.name == name))
            return;

        controller.AddParameter(name, type);
    }

    private static void UpdateKnifePrefab(
        Dictionary<string, Sprite> sprites
    )
    {
        const string prefabPath = "Assets/Prefabs/Knife.prefab";

        if (!File.Exists(prefabPath) || !sprites.ContainsKey("kunai"))
            return;

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = root.AddComponent<SpriteRenderer>();

        renderer.sprite = sprites["kunai"];
        renderer.color = Color.white;
        renderer.sortingOrder = 4;

        Rigidbody2D body = root.GetComponent<Rigidbody2D>();
        if (body == null)
            body = root.AddComponent<Rigidbody2D>();

        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = root.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
        collider.size = new Vector2(1.35f, 0.36f);

        root.transform.localScale = Vector3.one;

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void BuildScene(
        Scene scene,
        Dictionary<string, Sprite> sprites
    )
    {
        GameObject oldGenerated = GameObject.Find("PolishedLevel1");
        if (oldGenerated != null)
            UnityEngine.Object.DestroyImmediate(oldGenerated);

        GameObject oldGround = GameObject.Find("GroundTilemap");
        if (oldGround != null)
            oldGround.SetActive(false);

        GameObject player = GameObject.Find("player");
        if (player == null)
            throw new Exception("The player GameObject was not found.");

        ConfigurePlayer(player);

        GameObject cameraObject = GameObject.Find("Main Camera");
        if (cameraObject == null)
            throw new Exception("Main Camera was not found.");

        ConfigureCamera(cameraObject, player.transform);

        GameObject root = new GameObject("PolishedLevel1");

        CreateBackground(root.transform, sprites["background"]);
        CreateTerrain(root.transform, sprites);
        CreateDecorations(root.transform, sprites);
        CreateHazards(root.transform, sprites);
        CreateTargets(root.transform, sprites);
        CreateEnemies(root.transform, player, sprites);
        CreateCollectibles(root.transform, sprites);
        CreateMovingPlatforms(root.transform, sprites);

        GameObject managerObject =
            new GameObject("Level1 Game Manager");
        managerObject.transform.SetParent(root.transform);
        Level1GameManager manager =
            managerObject.AddComponent<Level1GameManager>();

        GameObject audioObject =
            new GameObject("Audio");
        audioObject.transform.SetParent(root.transform);
        KnifeNinjaAudio audio =
            audioObject.AddComponent<KnifeNinjaAudio>();
        AssignAudioClips(audio);

        LevelHUD hud = CreateHUD(root.transform);
        manager.SetHUD(hud);

        CreateExit(root.transform, sprites, manager);

        CreateFireflies(root.transform);

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(cameraObject);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void ConfigurePlayer(GameObject player)
    {
        player.transform.position = new Vector3(-3.5f, 1.3f, 0f);

        BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(0.62f, 1.35f);
            collider.offset = new Vector2(0f, -0.02f);
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;
        }

        Transform groundCheck = player.transform.Find("GroundCheck");
        if (groundCheck != null)
            groundCheck.localPosition = new Vector3(0f, -0.72f, 0f);

        Transform throwPoint = player.transform.Find("ThrowPoint");
        if (throwPoint != null)
            throwPoint.localPosition = new Vector3(0.72f, 0.08f, 0f);

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null)
            health = player.AddComponent<PlayerHealth>();

        health.maxHealth = 3;

        PlayerThrow thrower = player.GetComponent<PlayerThrow>();
        if (thrower != null)
        {
            thrower.maxKnives = 8;
            thrower.knivesRemaining = 8;
            thrower.knifeSpeed = 13.5f;
            thrower.throwCooldown = 0.28f;
        }

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.sortingOrder = 5;
    }

    private static void ConfigureCamera(
        GameObject cameraObject,
        Transform player
    )
    {
        Camera cam = cameraObject.GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5.3f;
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.17f);
        }

        CameraFollow2D follow =
            cameraObject.GetComponent<CameraFollow2D>();

        if (follow == null)
            follow = cameraObject.AddComponent<CameraFollow2D>();

        follow.target = player;
        follow.offset = new Vector2(1.5f, 1.2f);
        follow.smoothTime = 0.2f;
        follow.minBounds = new Vector2(-2f, 1.8f);
        follow.maxBounds = new Vector2(42f, 5.2f);
    }


    private static void ConfigureLighting()
    {
        GameObject lightObject = GameObject.Find("Global Light 2D");
        if (lightObject == null)
            return;

        Light2D light = lightObject.GetComponent<Light2D>();
        if (light == null)
            return;

        light.intensity = 0.72f;
        light.color = new Color(0.72f, 0.8f, 1f);
    }

    private static void CreateBackground(
        Transform parent,
        Sprite sprite
    )
    {
        GameObject go = new GameObject("Moonlit Background");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(20f, 4.2f, 5f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -100;
    }

    private static void CreateTerrain(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        Tile top = CreateTileAsset(
            TileFolder + "/MossTop.asset",
            sprites["tile_top"]
        );

        Tile fill = CreateTileAsset(
            TileFolder + "/StoneFill.asset",
            sprites["tile_fill"]
        );

        Tile crack = CreateTileAsset(
            TileFolder + "/StoneCrack.asset",
            sprites["tile_crack"]
        );

        GameObject gridObject =
            new GameObject("PolishedGrid", typeof(Grid));
        gridObject.transform.SetParent(parent);

        GameObject mapObject =
            new GameObject(
                "Terrain",
                typeof(Tilemap),
                typeof(TilemapRenderer),
                typeof(TilemapCollider2D)
            );

        mapObject.transform.SetParent(gridObject.transform);
        mapObject.layer = LayerMask.NameToLayer("Ground");

        Tilemap tilemap = mapObject.GetComponent<Tilemap>();
        TilemapRenderer renderer =
            mapObject.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;

        AddPlatform(tilemap, -6, 14, -1, 4, top, fill, crack);
        AddPlatform(tilemap, 10, 6, -2, 3, top, fill, crack);
        AddPlatform(tilemap, 17, 5, 0, 2, top, fill, crack);
        AddPlatform(tilemap, 24, 8, -1, 4, top, fill, crack);
        AddPlatform(tilemap, 34, 6, 0, 3, top, fill, crack);
        AddPlatform(tilemap, 42, 8, -1, 4, top, fill, crack);

        tilemap.RefreshAllTiles();
    }

    private static Tile CreateTileAsset(
        string path,
        Sprite sprite
    )
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);

        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }

        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        EditorUtility.SetDirty(tile);

        return tile;
    }

    private static void AddPlatform(
        Tilemap map,
        int left,
        int width,
        int topY,
        int depth,
        Tile top,
        Tile fill,
        Tile crack
    )
    {
        for (int x = left; x < left + width; x++)
        {
            map.SetTile(new Vector3Int(x, topY, 0), top);

            for (int y = topY - 1;
                 y >= topY - depth + 1;
                 y--)
            {
                Tile chosen = ((x + y) % 5 == 0)
                    ? crack
                    : fill;

                map.SetTile(
                    new Vector3Int(x, y, 0),
                    chosen
                );
            }
        }
    }

    private static void CreateDecorations(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        CreateSpriteObject(
            "Bamboo Left",
            sprites["bamboo"],
            new Vector3(-5f, 1.2f, 0f),
            parent,
            -2
        );

        CreateSpriteObject(
            "Bamboo Ridge",
            sprites["bamboo"],
            new Vector3(20.4f, 3.1f, 0f),
            parent,
            -2
        );

        CreateSpriteObject(
            "Bamboo Gate",
            sprites["bamboo"],
            new Vector3(40.5f, 1.3f, 0f),
            parent,
            -2
        );

        foreach (Vector3 p in new[]
        {
            new Vector3(4f, 0.12f, 0f),
            new Vector3(18f, 1.12f, 0f),
            new Vector3(27f, 0.12f, 0f),
            new Vector3(43.5f, 0.12f, 0f)
        })
        {
            GameObject lantern = CreateSpriteObject(
                "Lantern",
                sprites["lantern"],
                p,
                parent,
                2
            );

            GameObject lightObject = new GameObject("Warm Light");
            lightObject.transform.SetParent(lantern.transform);
            lightObject.transform.localPosition =
                new Vector3(0f, -0.1f, 0f);

            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = new Color(1f, 0.55f, 0.25f);
            light.intensity = 0.65f;
            light.pointLightInnerRadius = 0.4f;
            light.pointLightOuterRadius = 2.5f;
        }
    }

    private static void CreateHazards(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        foreach (Vector3 p in new[]
        {
            new Vector3(12.5f, -1.0f, 0f),
            new Vector3(28.5f, 0.0f, 0f),
            new Vector3(36.5f, 1.0f, 0f)
        })
        {
            GameObject spike = CreateSpriteObject(
                "Spikes",
                sprites["spikes"],
                p,
                parent,
                3
            );

            BoxCollider2D c = spike.AddComponent<BoxCollider2D>();
            c.isTrigger = true;
            c.size = new Vector2(0.9f, 0.55f);
            c.offset = new Vector2(0f, -0.18f);

            Hazard hazard = spike.AddComponent<Hazard>();
            hazard.damage = 1;
            hazard.instantKill = false;
        }

        GameObject pit = new GameObject("Death Pit");
        pit.transform.SetParent(parent);
        pit.transform.position = new Vector3(21f, -6.2f, 0f);

        BoxCollider2D pitCollider = pit.AddComponent<BoxCollider2D>();
        pitCollider.isTrigger = true;
        pitCollider.size = new Vector2(70f, 2f);

        Hazard pitHazard = pit.AddComponent<Hazard>();
        pitHazard.instantKill = true;
    }

    private static void CreateTargets(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        foreach (Vector3 p in new[]
        {
            new Vector3(5.2f, 0.25f, 0f),
            new Vector3(20f, 1.25f, 0f),
            new Vector3(37.4f, 1.25f, 0f)
        })
        {
            GameObject target = CreateSpriteObject(
                "Training Target",
                sprites["target"],
                p,
                parent,
                3
            );

            BoxCollider2D c = target.AddComponent<BoxCollider2D>();
            c.isTrigger = true;
            c.size = new Vector2(0.85f, 1.35f);

            target.AddComponent<TargetDummy>();
        }
    }

    private static void CreateEnemies(
        Transform parent,
        GameObject player,
        Dictionary<string, Sprite> sprites
    )
    {
        SpriteRenderer playerRenderer =
            player.GetComponent<SpriteRenderer>();

        if (playerRenderer == null ||
            playerRenderer.sprite == null)
            return;

        CreateEnemy(
            parent,
            playerRenderer.sprite,
            new Vector2(12.5f, -0.72f),
            new Vector2(14.7f, -0.72f)
        );

        CreateEnemy(
            parent,
            playerRenderer.sprite,
            new Vector2(26f, 0.28f),
            new Vector2(30f, 0.28f)
        );
    }

    private static void CreateEnemy(
        Transform parent,
        Sprite sprite,
        Vector2 a,
        Vector2 b
    )
    {
        GameObject enemy = new GameObject("Crimson Ninja");
        enemy.transform.SetParent(parent);
        enemy.transform.position = a;

        SpriteRenderer renderer =
            enemy.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.95f, 0.33f, 0.38f);
        renderer.sortingOrder = 4;

        BoxCollider2D collider =
            enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.62f, 1.3f);

        EnemyPatrol patrol = enemy.AddComponent<EnemyPatrol>();
        patrol.pointA = a;
        patrol.pointB = b;
        patrol.speed = 1.15f;
    }

    private static void CreateCollectibles(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        foreach (Vector3 p in new[]
        {
            new Vector3(1.5f, 0.8f, 0f),
            new Vector3(8.7f, 1.5f, 0f),
            new Vector3(13f, -0.2f, 0f),
            new Vector3(18.5f, 1.8f, 0f),
            new Vector3(25.5f, 0.8f, 0f),
            new Vector3(32.6f, 2.2f, 0f),
            new Vector3(35.5f, 1.8f, 0f),
            new Vector3(44.5f, 0.8f, 0f)
        })
        {
            GameObject coin = CreateSpriteObject(
                "Moon Coin",
                sprites["coin"],
                p,
                parent,
                4
            );

            CircleCollider2D c =
                coin.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.35f;

            coin.AddComponent<CollectibleCoin>();
        }

        foreach (Vector3 p in new[]
        {
            new Vector3(11.2f, -1.0f, 0f),
            new Vector3(27.3f, 0.0f, 0f)
        })
        {
            GameObject crate = CreateSpriteObject(
                "Knife Crate",
                sprites["knife_crate"],
                p,
                parent,
                3
            );

            BoxCollider2D c = crate.AddComponent<BoxCollider2D>();
            c.isTrigger = true;
            c.size = new Vector2(0.9f, 0.9f);

            crate.AddComponent<KnifePickup>();
        }
    }

    private static void CreateMovingPlatforms(
        Transform parent,
        Dictionary<string, Sprite> sprites
    )
    {
        GameObject platform = new GameObject("Moving Platform");
        platform.transform.SetParent(parent);
        platform.transform.position = new Vector3(32.5f, 0.8f, 0f);

        for (int i = -1; i <= 1; i++)
        {
            GameObject piece = new GameObject("Wood Tile");
            piece.transform.SetParent(platform.transform);
            piece.transform.localPosition = new Vector3(i, 0f, 0f);

            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = sprites["wood_platform"];
            sr.sortingOrder = 2;
        }

        BoxCollider2D collider =
            platform.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(3f, 0.45f);

        Rigidbody2D rb = platform.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        MovingPlatform movement =
            platform.AddComponent<MovingPlatform>();
        movement.pointA = new Vector2(32.5f, 0.8f);
        movement.pointB = new Vector2(32.5f, 3.1f);
        movement.speed = 1.1f;

        platform.layer = LayerMask.NameToLayer("Ground");
    }

    private static void CreateExit(
        Transform parent,
        Dictionary<string, Sprite> sprites,
        Level1GameManager manager
    )
    {
        GameObject gate = CreateSpriteObject(
            "Shrine Gate",
            sprites["gate"],
            new Vector3(47f, 1.5f, 0f),
            parent,
            2
        );

        GameObject barrier = new GameObject("Sealed Gate Barrier");
        barrier.transform.SetParent(gate.transform);
        barrier.transform.localPosition = Vector3.zero;

        BoxCollider2D block = barrier.AddComponent<BoxCollider2D>();
        block.size = new Vector2(0.7f, 3.2f);
        block.isTrigger = false;

        manager.SetGateBarrier(barrier);

        GameObject exit = new GameObject("Level Exit");
        exit.transform.SetParent(gate.transform);
        exit.transform.localPosition = new Vector3(0.75f, 0f, 0f);

        BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.2f, 3.2f);

        exit.AddComponent<LevelExit>();
    }

    private static void CreateFireflies(Transform parent)
    {
        GameObject go = new GameObject("Fireflies");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(20f, 2.5f, 0f);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
        main.startColor = new Color(0.65f, 1f, 0.65f, 0.65f);
        main.maxParticles = 90;

        var emission = ps.emission;
        emission.rateOverTime = 7f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(55f, 8f, 0.1f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.25f;

        ps.Play();
    }

    private static LevelHUD CreateHUD(Transform parent)
    {
        GameObject canvasObject = new GameObject(
            "HUD",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(LevelHUD)
        );

        canvasObject.transform.SetParent(parent);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        LevelHUD hud = canvasObject.GetComponent<LevelHUD>();

        Font font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );

        GameObject infoPanel = CreatePanel(
            "Info Panel",
            canvasObject.transform,
            new Color(0.035f, 0.05f, 0.1f, 0.83f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -24f),
            new Vector2(600f, 156f)
        );

        Text title = CreateText(
            "Title",
            infoPanel.transform,
            font,
            27,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Vector2(24f, -16f),
            new Vector2(550f, 38f)
        );

        Text status = CreateText(
            "Status",
            infoPanel.transform,
            font,
            23,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Vector2(24f, -58f),
            new Vector2(550f, 34f)
        );

        Text objective = CreateText(
            "Objective",
            infoPanel.transform,
            font,
            18,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(24f, -98f),
            new Vector2(550f, 46f)
        );

        Text controls = CreateText(
            "Controls",
            canvasObject.transform,
            font,
            19,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            new Vector2(0f, -24f),
            new Vector2(720f, 40f),
            new Vector2(0.5f, 1f)
        );
        controls.text = "A / D  MOVE     SPACE  JUMP     F  THROW";
        controls.color = new Color(0.9f, 0.94f, 1f, 0.9f);

        Text credits = CreateText(
            "Credits",
            canvasObject.transform,
            font,
            16,
            FontStyle.Normal,
            TextAnchor.LowerRight,
            new Vector2(-24f, 20f),
            new Vector2(520f, 34f),
            new Vector2(1f, 0f)
        );
        credits.text = "Developed by Ishan, Ayush & Meheraj";
        credits.color = new Color(0.8f, 0.84f, 0.92f, 0.82f);

        Text message = CreateText(
            "Message",
            canvasObject.transform,
            font,
            25,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 130f),
            new Vector2(900f, 60f),
            new Vector2(0.5f, 0f)
        );
        message.color = new Color(1f, 0.83f, 0.42f);
        message.gameObject.SetActive(false);

        GameObject winPanel = CreatePanel(
            "Win Panel",
            canvasObject.transform,
            new Color(0.02f, 0.03f, 0.08f, 0.92f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(780f, 390f)
        );

        Text winTitle = CreateText(
            "Win Title",
            winPanel.transform,
            font,
            52,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 90f),
            new Vector2(720f, 80f),
            new Vector2(0.5f, 0.5f)
        );
        winTitle.color = new Color(0.95f, 0.78f, 0.34f);

        Text winDetail = CreateText(
            "Win Detail",
            winPanel.transform,
            font,
            27,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -40f),
            new Vector2(700f, 190f),
            new Vector2(0.5f, 0.5f)
        );

        winPanel.SetActive(false);

        hud.titleText = title;
        hud.statusText = status;
        hud.objectiveText = objective;
        hud.messageText = message;
        hud.winPanel = winPanel;
        hud.winTitleText = winTitle;
        hud.winDetailText = winDetail;
        hud.creditsText = credits;

        return hud;
    }

    private static GameObject CreatePanel(
        string name,
        Transform parent,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 size
    )
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        image.color = color;

        return go;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        Font font,
        int fontSize,
        FontStyle style,
        TextAnchor alignment,
        Vector2 anchoredPosition,
        Vector2 size,
        Vector2? anchor = null
    )
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Text)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        Vector2 a = anchor ?? new Vector2(0f, 1f);
        rect.anchorMin = a;
        rect.anchorMax = a;
        rect.pivot = a;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private static GameObject CreateSpriteObject(
        string name,
        Sprite sprite,
        Vector3 position,
        Transform parent,
        int sortingOrder
    )
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        return go;
    }

    private static void AssignAudioClips(KnifeNinjaAudio audio)
    {
        audio.levelTheme = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/level_theme.wav"
        );
        audio.jumpClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/jump.wav"
        );
        audio.throwClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/throw.wav"
        );
        audio.hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/hit.wav"
        );
        audio.coinClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/coin.wav"
        );
        audio.hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/hurt.wav"
        );
        audio.gateClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/gate.wav"
        );
        audio.winClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            AudioFolder + "/win.wav"
        );
    }

    private static void EnsureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes =
            EditorBuildSettings.scenes.ToList();

        scenes.RemoveAll(s => s.path == ScenePath);
        scenes.Insert(
            0,
            new EditorBuildSettingsScene(ScenePath, true)
        );

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
