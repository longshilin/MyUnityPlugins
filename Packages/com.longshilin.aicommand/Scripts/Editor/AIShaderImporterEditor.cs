using Longsl.AICommand.OpenAI;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Longsl.AICommand
{
    [CustomEditor(typeof(AIShaderImporter))]
    sealed class AIShaderImporterEditor : ScriptedImporterEditor
    {
        #region Private members

        SerializedProperty _prompt;
        SerializedProperty _cached;

        bool IsApiKeyOk
            => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey) ||
               !string.IsNullOrEmpty(OpenAIUtil.CHATGPT_KEY);

        const string ApiKeyErrorText =
            "API Key hasn't been set. Please check the project settings (Edit > Project Settings > AI Shader > API Key).";

        static string WrapPrompt(string input) => "Create an unlit shader for Unity. " + input +
                                                  " Don't include any note nor explanation in the response." +
                                                  " I only need the code body.";

        void Regenerate() => _cached.stringValue = OpenAIUtil.InvokeChat(WrapPrompt(_prompt.stringValue));

        #endregion

        #region ScriptedImporterEditor overrides

        public override void OnEnable()
        {
            base.OnEnable();
            _prompt = serializedObject.FindProperty("_prompt");
            _cached = serializedObject.FindProperty("_cached");

            if (string.IsNullOrEmpty(AICommandSettings.instance.apiKey))
            {
                string value =
                    System.Environment.GetEnvironmentVariable("CHATGPT_KEY", System.EnvironmentVariableTarget.User);

                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogWarning("The user environment variable [CHATGPT_KEY] does not exist");
                }
                else
                {
                    OpenAIUtil.CHATGPT_KEY = value;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // Intro
            serializedObject.Update();

            // Prompt text area
            EditorGUILayout.PropertyField(_prompt);

            // "Generate" button
            EditorGUI.BeginDisabledGroup(!IsApiKeyOk);
            if (GUILayout.Button("Generate")) Regenerate();
            EditorGUI.EndDisabledGroup();

            // Missing API key error
            if (!IsApiKeyOk) EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);

            // Cached code text area
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_cached);

            // Outro
            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        #endregion
    }
} // namespace AIShader