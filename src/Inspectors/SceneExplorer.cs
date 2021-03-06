﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityExplorer.UI;
using UnityExplorer.UI.Modules;
using UnityExplorer.UI.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityExplorer.Unstrip;
using UnityExplorer.Helpers;

namespace UnityExplorer.Inspectors
{
    public class SceneExplorer
    {
        public static SceneExplorer Instance;

        internal static Action OnToggleShow;

        public SceneExplorer()
        { 
            Instance = this; 
            ConstructScenePane();
        }

        private static bool Hiding;

        private const float UPDATE_INTERVAL = 1f;
        private float m_timeOfLastSceneUpdate;

        // private int m_currentSceneHandle = -1;
        public static Scene DontDestroyScene => DontDestroyObject.scene;
        internal Scene m_currentScene;
        internal Scene[] m_currentScenes = new Scene[0];

        private GameObject m_selectedSceneObject;
        private int m_lastCount;

        private Dropdown m_sceneDropdown;
        private Text m_sceneDropdownText;
        private Text m_scenePathText;
        private GameObject m_mainInspectBtn;
        private GameObject m_backButtonObj;

        public PageHandler m_pageHandler;
        private GameObject m_pageContent;
        private GameObject[] m_allObjects = new GameObject[0];
        private readonly List<GameObject> m_shortList = new List<GameObject>();
        private readonly List<Text> m_shortListTexts = new List<Text>();
        private readonly List<Toggle> m_shortListToggles = new List<Toggle>();


        internal static GameObject DontDestroyObject
        {
            get
            {
                if (!s_dontDestroyObject)
                {
                    s_dontDestroyObject = new GameObject("DontDestroyMe");
                    GameObject.DontDestroyOnLoad(s_dontDestroyObject);
                }
                return s_dontDestroyObject;
            }
        }
        internal static GameObject s_dontDestroyObject;

        public void Init()
        {
            RefreshSceneSelector();
        }

        public void Update()
        {
            if (Hiding || Time.realtimeSinceStartup - m_timeOfLastSceneUpdate < UPDATE_INTERVAL)
            {
                return;
            }

            RefreshSceneSelector();

            if (!m_selectedSceneObject)
            {
                if (m_currentScene != default)
                {
#if CPP
                    SetSceneObjectList(SceneUnstrip.GetRootGameObjects(m_currentScene.handle));
#else
                    SetSceneObjectList(m_currentScene.GetRootGameObjects());
#endif
                }
            }
            else
            {
                RefreshSelectedSceneObject();
            }
        }

        internal void OnSceneChange()
        {
            m_sceneDropdown.OnCancel(null);
            RefreshSceneSelector();
        }

        private void RefreshSceneSelector()
        {
            var newNames = new List<string>();
            var newScenes = new List<Scene>();

            if (m_currentScenes == null)
                m_currentScenes = new Scene[0];

            bool anyChange = SceneManager.sceneCount != m_currentScenes.Length - 1;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene == default)
                    continue;

                if (!anyChange && !m_currentScenes.Any(it => it.GetHandle() == scene.GetHandle()))
                    anyChange = true;

                newScenes.Add(scene);
                newNames.Add(scene.name);
            }

            newNames.Add("DontDestroyOnLoad");
            newScenes.Add(DontDestroyScene);

            m_sceneDropdown.options.Clear();

            foreach (string scene in newNames)
            {
                m_sceneDropdown.options.Add(new Dropdown.OptionData { text = scene });
            }

            if (anyChange)
            {
                m_sceneDropdownText.text = newNames[0];
                SetTargetScene(newScenes[0]);
            }

            m_currentScenes = newScenes.ToArray();
        }

        public void SetTargetScene(Scene scene)
        {
            if (scene == default)
                return;

            m_currentScene = scene;
#if CPP
            GameObject[] rootObjs = SceneUnstrip.GetRootGameObjects(scene.handle);
#else
            GameObject[] rootObjs = scene.GetRootGameObjects();
#endif
            SetSceneObjectList(rootObjs);

            m_selectedSceneObject = null;

            if (m_backButtonObj.activeSelf)
            {
                m_backButtonObj.SetActive(false);
                m_mainInspectBtn.SetActive(false);
            }

            m_scenePathText.text = "Scene root:";
            //m_scenePathText.ForceMeshUpdate();
        }

        public void SetTargetObject(GameObject obj)
        {
            if (!obj)
                return;

            m_scenePathText.text = obj.name;
            //m_scenePathText.ForceMeshUpdate();

            m_selectedSceneObject = obj;

            RefreshSelectedSceneObject();

            if (!m_backButtonObj.activeSelf)
            {
                m_backButtonObj.SetActive(true);
                m_mainInspectBtn.SetActive(true);
            }
        }

        private void RefreshSelectedSceneObject()
        {
            GameObject[] list = new GameObject[m_selectedSceneObject.transform.childCount];
            for (int i = 0; i < m_selectedSceneObject.transform.childCount; i++)
            {
                list[i] = m_selectedSceneObject.transform.GetChild(i).gameObject;
            }

            SetSceneObjectList(list);
        }

        private void SetSceneObjectList(GameObject[] objects)
        {
            m_allObjects = objects;
            RefreshSceneObjectList();
        }

        private void SceneListObjectClicked(int index)
        {
            if (index >= m_shortList.Count || !m_shortList[index])
            {
                return;
            }

            var obj = m_shortList[index];
            if (obj.transform.childCount > 0)
                SetTargetObject(obj);
            else
                InspectorManager.Instance.Inspect(obj);
        }

        private void OnSceneListPageTurn()
        {
            RefreshSceneObjectList();
        }

        private void OnToggleClicked(int index, bool val)
        {
            if (index >= m_shortList.Count || !m_shortList[index])
                return;

            var obj = m_shortList[index];
            obj.SetActive(val);
        }

        private void RefreshSceneObjectList()
        {
            m_timeOfLastSceneUpdate = Time.realtimeSinceStartup;

            var objects = m_allObjects;
            m_pageHandler.ListCount = objects.Length;

            //int startIndex = m_sceneListPageHandler.StartIndex;

            int newCount = 0;

            foreach (var itemIndex in m_pageHandler)
            {
                newCount++;

                // normalized index starting from 0
                var i = itemIndex - m_pageHandler.StartIndex;

                if (itemIndex >= objects.Length)
                {
                    if (i > m_lastCount || i >= m_shortListTexts.Count)
                        break;

                    GameObject label = m_shortListTexts[i].transform.parent.parent.gameObject;
                    if (label.activeSelf)
                        label.SetActive(false);
                }
                else
                {
                    GameObject obj = objects[itemIndex];

                    if (!obj)
                        continue;

                    if (i >= m_shortList.Count)
                    {
                        m_shortList.Add(obj);
                        AddObjectListButton();
                    }
                    else
                    {
                        m_shortList[i] = obj;
                    }

                    var text = m_shortListTexts[i];

                    var name = obj.name;

                    if (obj.transform.childCount > 0)
                        name = $"<color=grey>[{obj.transform.childCount}]</color> {name}";

                    text.text = name;
                    text.color = obj.activeSelf ? Color.green : Color.red;

                    var tog = m_shortListToggles[i];
                    tog.isOn = obj.activeSelf;

                    var label = text.transform.parent.parent.gameObject;
                    if (!label.activeSelf)
                    {
                        label.SetActive(true);
                    }
                }
            }

            m_lastCount = newCount;
        }

#region UI CONSTRUCTION

        public void ConstructScenePane()
        {
            GameObject leftPane = UIFactory.CreateVerticalGroup(HomePage.Instance.Content, new Color(72f / 255f, 72f / 255f, 72f / 255f));
            LayoutElement leftLayout = leftPane.AddComponent<LayoutElement>();
            leftLayout.minWidth = 350;
            leftLayout.flexibleWidth = 0;

            VerticalLayoutGroup leftGroup = leftPane.GetComponent<VerticalLayoutGroup>();
            leftGroup.padding.left = 4;
            leftGroup.padding.right = 4;
            leftGroup.padding.top = 8;
            leftGroup.padding.bottom = 4;
            leftGroup.spacing = 4;
            leftGroup.childControlWidth = true;
            leftGroup.childControlHeight = true;
            leftGroup.childForceExpandWidth = true;
            leftGroup.childForceExpandHeight = true;

            GameObject titleObj = UIFactory.CreateLabel(leftPane, TextAnchor.UpperLeft);
            Text titleLabel = titleObj.GetComponent<Text>();
            titleLabel.text = "Scene Explorer";
            titleLabel.fontSize = 20;
            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.minHeight = 30;
            titleLayout.flexibleHeight = 0;

            GameObject sceneDropdownObj = UIFactory.CreateDropdown(leftPane, out m_sceneDropdown);
            LayoutElement dropdownLayout = sceneDropdownObj.AddComponent<LayoutElement>();
            dropdownLayout.minHeight = 40;
            dropdownLayout.flexibleHeight = 0;
            dropdownLayout.minWidth = 320;
            dropdownLayout.flexibleWidth = 2;

            m_sceneDropdownText = m_sceneDropdown.transform.Find("Label").GetComponent<Text>();
            m_sceneDropdown.onValueChanged.AddListener((int val) => { SetSceneFromDropdown(val); });

            void SetSceneFromDropdown(int val)
            {
                //string scene = m_sceneDropdown.options[val].text;
                SetTargetScene(m_currentScenes[val]);
            }

            GameObject scenePathGroupObj = UIFactory.CreateHorizontalGroup(leftPane, new Color(1, 1, 1, 0f));
            HorizontalLayoutGroup scenePathGroup = scenePathGroupObj.GetComponent<HorizontalLayoutGroup>();
            scenePathGroup.childControlHeight = true;
            scenePathGroup.childControlWidth = true;
            scenePathGroup.childForceExpandHeight = true;
            scenePathGroup.childForceExpandWidth = true;
            scenePathGroup.spacing = 5;
            LayoutElement scenePathLayout = scenePathGroupObj.AddComponent<LayoutElement>();
            scenePathLayout.minHeight = 20;
            scenePathLayout.minWidth = 335;
            scenePathLayout.flexibleWidth = 0;

            m_backButtonObj = UIFactory.CreateButton(scenePathGroupObj);
            Text backButtonText = m_backButtonObj.GetComponentInChildren<Text>();
            backButtonText.text = "◄";
            LayoutElement backButtonLayout = m_backButtonObj.AddComponent<LayoutElement>();
            backButtonLayout.minWidth = 40;
            backButtonLayout.flexibleWidth = 0;
            Button backButton = m_backButtonObj.GetComponent<Button>();
            var colors = backButton.colors;
            colors.normalColor = new Color(0.12f, 0.12f, 0.12f);
            backButton.colors = colors;

            backButton.onClick.AddListener(() => { SetSceneObjectParent(); });

            void SetSceneObjectParent()
            {
                if (!m_selectedSceneObject || !m_selectedSceneObject.transform.parent?.gameObject)
                {
                    m_selectedSceneObject = null;
                    SetTargetScene(m_currentScene);
                }
                else
                {
                    SetTargetObject(m_selectedSceneObject.transform.parent.gameObject);
                }
            }

            GameObject scenePathLabel = UIFactory.CreateHorizontalGroup(scenePathGroupObj);
            Image image = scenePathLabel.GetComponent<Image>();
            image.color = Color.white;

            LayoutElement scenePathLabelLayout = scenePathLabel.AddComponent<LayoutElement>();
            scenePathLabelLayout.minWidth = 210;
            scenePathLabelLayout.minHeight = 20;
            scenePathLabelLayout.flexibleHeight = 0;
            scenePathLabelLayout.flexibleWidth = 120;

            scenePathLabel.AddComponent<Mask>().showMaskGraphic = false;

            GameObject scenePathLabelText = UIFactory.CreateLabel(scenePathLabel, TextAnchor.MiddleLeft);
            m_scenePathText = scenePathLabelText.GetComponent<Text>();
            m_scenePathText.text = "Scene root:";
            m_scenePathText.fontSize = 15;
            m_scenePathText.horizontalOverflow = HorizontalWrapMode.Overflow;

            LayoutElement textLayout = scenePathLabelText.gameObject.AddComponent<LayoutElement>();
            textLayout.minWidth = 210;
            textLayout.flexibleWidth = 120;
            textLayout.minHeight = 20;
            textLayout.flexibleHeight = 0;

            m_mainInspectBtn = UIFactory.CreateButton(scenePathGroupObj);
            Text inspectButtonText = m_mainInspectBtn.GetComponentInChildren<Text>();
            inspectButtonText.text = "Inspect";
            LayoutElement inspectButtonLayout = m_mainInspectBtn.AddComponent<LayoutElement>();
            inspectButtonLayout.minWidth = 65;
            inspectButtonLayout.flexibleWidth = 0;
            Button inspectButton = m_mainInspectBtn.GetComponent<Button>();
            colors = inspectButton.colors;
            colors.normalColor = new Color(0.12f, 0.12f, 0.12f);
            inspectButton.colors = colors;

            inspectButton.onClick.AddListener(() => { InspectorManager.Instance.Inspect(m_selectedSceneObject); });

            GameObject scrollObj = UIFactory.CreateScrollView(leftPane, out m_pageContent, out SliderScrollbar scroller, new Color(0.1f, 0.1f, 0.1f));

            m_pageHandler = new PageHandler(scroller);
            m_pageHandler.ConstructUI(leftPane);
            m_pageHandler.OnPageChanged += OnSceneListPageTurn;

            // hide button

            var hideButtonObj = UIFactory.CreateButton(leftPane);
            var hideBtn = hideButtonObj.GetComponent<Button>();

            var hideColors = hideBtn.colors;
            hideColors.normalColor = new Color(0.15f, 0.15f, 0.15f);
            hideBtn.colors = hideColors;
            var hideText = hideButtonObj.GetComponentInChildren<Text>();
            hideText.text = "Hide Scene Explorer";
            hideText.fontSize = 13;
            var hideLayout = hideButtonObj.AddComponent<LayoutElement>();
            hideLayout.minWidth = 20;
            hideLayout.minHeight = 20;

            hideBtn.onClick.AddListener(OnHide);

            void OnHide()
            {
                if (!Hiding)
                {
                    Hiding = true;

                    hideText.text = "►";
                    titleObj.SetActive(false);
                    sceneDropdownObj.SetActive(false);
                    scenePathGroupObj.SetActive(false);
                    scrollObj.SetActive(false);
                    m_pageHandler.Hide();

                    leftLayout.minWidth = 15;
                }
                else
                {
                    Hiding = false;

                    hideText.text = "Hide Scene Explorer";
                    titleObj.SetActive(true);
                    sceneDropdownObj.SetActive(true);
                    scenePathGroupObj.SetActive(true);
                    scrollObj.SetActive(true);

                    leftLayout.minWidth = 350;

                    Update();
                }

                OnToggleShow?.Invoke();
            }
        }

        private void AddObjectListButton()
        {
            int thisIndex = m_shortListTexts.Count();

            GameObject btnGroupObj = UIFactory.CreateHorizontalGroup(m_pageContent, new Color(0.1f, 0.1f, 0.1f));
            HorizontalLayoutGroup btnGroup = btnGroupObj.GetComponent<HorizontalLayoutGroup>();
            btnGroup.childForceExpandWidth = true;
            btnGroup.childControlWidth = true;
            btnGroup.childForceExpandHeight = false;
            btnGroup.childControlHeight = true;
            LayoutElement btnLayout = btnGroupObj.AddComponent<LayoutElement>();
            btnLayout.flexibleWidth = 320;
            btnLayout.minHeight = 25;
            btnLayout.flexibleHeight = 0;
            btnGroupObj.AddComponent<Mask>();

            var toggleObj = UIFactory.CreateToggle(btnGroupObj, out Toggle toggle, out Text toggleText, new Color(0.1f, 0.1f, 0.1f));
            var toggleLayout = toggleObj.AddComponent<LayoutElement>();
            toggleLayout.minHeight = 25;
            toggleLayout.minWidth = 25;
            toggleText.text = "";
            toggle.isOn = false;
            m_shortListToggles.Add(toggle);
            toggle.onValueChanged.AddListener((bool val) => { OnToggleClicked(thisIndex, val); });

            GameObject mainButtonObj = UIFactory.CreateButton(btnGroupObj);
            LayoutElement mainBtnLayout = mainButtonObj.AddComponent<LayoutElement>();
            mainBtnLayout.minHeight = 25;
            mainBtnLayout.flexibleHeight = 0;
            mainBtnLayout.minWidth = 230;
            mainBtnLayout.flexibleWidth = 0;
            Button mainBtn = mainButtonObj.GetComponent<Button>();
            ColorBlock mainColors = mainBtn.colors;
            mainColors.normalColor = new Color(0.1f, 0.1f, 0.1f);
            mainColors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1);
            mainBtn.colors = mainColors;

            mainBtn.onClick.AddListener(() => { SceneListObjectClicked(thisIndex); });

            Text mainText = mainButtonObj.GetComponentInChildren<Text>();
            mainText.alignment = TextAnchor.MiddleLeft;
            mainText.horizontalOverflow = HorizontalWrapMode.Overflow;
            m_shortListTexts.Add(mainText);

            GameObject inspectBtnObj = UIFactory.CreateButton(btnGroupObj);
            LayoutElement inspectBtnLayout = inspectBtnObj.AddComponent<LayoutElement>();
            inspectBtnLayout.minWidth = 60;
            inspectBtnLayout.flexibleWidth = 0;
            inspectBtnLayout.minHeight = 25;
            inspectBtnLayout.flexibleHeight = 0;
            Text inspectText = inspectBtnObj.GetComponentInChildren<Text>();
            inspectText.text = "Inspect";
            inspectText.color = Color.white;

            Button inspectBtn = inspectBtnObj.GetComponent<Button>();
            ColorBlock inspectColors = inspectBtn.colors;
            inspectColors.normalColor = new Color(0.15f, 0.15f, 0.15f);
            mainColors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            inspectBtn.colors = inspectColors;

            inspectBtn.onClick.AddListener(() => { InspectorManager.Instance.Inspect(m_shortList[thisIndex]); });
        }

#endregion
    }
}
