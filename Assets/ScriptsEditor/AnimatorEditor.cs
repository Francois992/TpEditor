using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimatorEditor : EditorWindow
{
    

    [MenuItem("Window/AnimatorManager")]
    static void Init()
    {        
        AnimatorEditor window = GetWindow<AnimatorEditor>();
    }

    static Animator[] animators;
    static Animator selectedAnimator = null;
    static AnimationClip selectedAnim = null;

    bool isAnimatorOpen = false;
    bool isAnimOpen = false;

    bool isSimulatingAnim = false;
    bool loopSimulation = false;
    float loopDelay = 0;
    float delayTime = 0;

    float lastEditortime = 0;
    float animationSpeed = 1;
    float animTime = 0;

    private void OnEnable()
    {
        //Init();
        animators = FindObjectsOfType<Animator>();
        EditorSceneManager.sceneOpened += ChangeScene;
        EditorApplication.playModeStateChanged += StartPlay;
        EditorApplication.hierarchyChanged += HierarchyChange;
    }

    private void OnDisable()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= EditorAnimUpdate;
    }

    private void OnGUI()
    {
        SearchField search = new SearchField();
        
        search.OnGUI("search Animators (W.I.P.)");
        if (search.HasFocus())
        {
            
        }

        GUILayout.Label("Animators", EditorStyles.boldLabel);
        

        if (animators.Length == 0) return;

        for(int i = 0; i < animators.Length; i++)
        {
            GUIContent content = new GUIContent();
            content.text = animators[i].name;
            if (GUILayout.Button(content))
            {

                if (!isAnimatorOpen || selectedAnimator != animators[i].gameObject) isAnimatorOpen = true;
                else isAnimatorOpen = false;
                selectedAnimator = animators[i];
                
                Selection.activeObject = animators[i];               
            }
            if(selectedAnimator != null)
            {
                if (selectedAnimator == animators[i])
                {
                    if (isAnimatorOpen) DrawAnimButtons();
                }
            }
            
        }

        
    }

    void DrawAnimButtons()
    {
        if(selectedAnimator != null)
        {
            GUILayout.BeginVertical();
            AnimationClip[] animationClips = selectedAnimator.GetComponent<Animator>().runtimeAnimatorController.animationClips;

            if (animationClips.Length == 0) return;

            for (int j = 0; j < animationClips.Length; j++)
            {
                GUIContent animContent = new GUIContent();
                animContent.text = animationClips[j].name;
                if (GUILayout.Button(animContent, GUILayout.MaxWidth(300), GUILayout.Width(150)))
                {
                    if (!isAnimOpen || selectedAnim != animationClips[j])
                    {
                        reset();
                        isAnimOpen = true;
                    }
                    else isAnimOpen = false;
                    selectedAnim = animationClips[j];
                }
                if (selectedAnim == animationClips[j])
                {
                    if (isAnimOpen) OnSelectedAnim(animationClips[j]);
                }
            }
            GUILayout.EndVertical();

        }
    }

    public void OnSelectedAnim(object anim)
    {
        GUILayout.BeginVertical();
        
        if (GUILayout.Button(new GUIContent("Play"), GUILayout.Width(100)))
        {
            if(!isSimulatingAnim && Application.isEditor) PlayAnim();
        }
        if (GUILayout.Button(new GUIContent("Stop"), GUILayout.Width(100)))
        {
            if (isSimulatingAnim && Application.isEditor) StopAnim();
        }
        animationSpeed = EditorGUILayout.FloatField(new GUIContent("AnimationSpeed :"),animationSpeed);
        animationSpeed = Mathf.Clamp(animationSpeed, 0, 5);

        GUILayout.Label("Frame selection");

        animTime = GUILayout.HorizontalSlider(animTime, 0, selectedAnim.length);
        GUILayout.Space(15f);
        selectedAnim.SampleAnimation(selectedAnimator.gameObject, animTime);

        loopSimulation = GUILayout.Toggle(loopSimulation, new GUIContent("Make animation loop"));

        if(loopSimulation)
        {
            loopDelay = EditorGUILayout.FloatField(new GUIContent("Delay between loops :"), loopDelay);
            loopDelay = Mathf.Clamp(loopDelay, 0, 5);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Box(new GUIContent("Total duration in seconds : " + selectedAnim.length.ToString()));
        GUILayout.Box(new GUIContent("Is looping in game : " + (selectedAnim.isLooping ? "true" : "false")));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void PlayAnim()
    {
        AnimationMode.StartAnimationMode();
        EditorApplication.update -= EditorAnimUpdate;
        EditorApplication.update += EditorAnimUpdate;
        lastEditortime = Time.realtimeSinceStartup;
        isSimulatingAnim = true;
    }

    void StopAnim()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= EditorAnimUpdate;
        isSimulatingAnim = false;
    }

    void EditorAnimUpdate()
    {  
        if(selectedAnimator != null && selectedAnim != null)
        {
            float deltaTime = Time.realtimeSinceStartup - lastEditortime;
            AnimationMode.SampleAnimationClip(selectedAnimator.gameObject, selectedAnim, time: deltaTime * animationSpeed);
            if (loopSimulation && deltaTime >= selectedAnim.length)
            {
                delayTime = deltaTime - 1;

                if (delayTime >= loopDelay)
                {
                    lastEditortime = Time.realtimeSinceStartup;
                    delayTime = 0;
                }
            }
        }
        
    }

    static void StartPlay(PlayModeStateChange state)
    {
        AnimationMode.StopAnimationMode();
    }

    static void ChangeScene(Scene scene, OpenSceneMode mode)
    {
        AnimationMode.StopAnimationMode();
        animators = FindObjectsOfType<Animator>();
    }

    static void HierarchyChange()
    {
        animators = FindObjectsOfType<Animator>();
    }

    void reset()
    {
        StopAnim();
        loopSimulation = false;
        animationSpeed = 1;
        animTime = 0;
        loopDelay = 0;
        delayTime = 0;
    }
}
