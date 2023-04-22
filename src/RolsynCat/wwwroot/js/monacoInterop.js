
import * as module from "./module.js";

let languageId = "csharp";
let monacoInterop = {};
monacoInterop.editors = {};
let csharpEditor = {};
let defaultCode =
    [
        `using System;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("欢迎使用RoslynCat");
    }
}`
    ].join('\n');
let sourceCode = localStorage.getItem('oldCode') ?? defaultCode;

monacoInterop.getOldCode = () => {
    return localStorage.getItem('oldCode');
}
//创建和初始化编辑器
monacoInterop.createEditor = (elementId, code) => {
    let editor;
    if (elementId == 'editorId') {
        if (code != defaultCode) {
            sourceCode = code;
        }
        editor = module.createEditor(elementId, sourceCode);
        csharpEditor = editor;
        window.csharpEditor = editor;

        monacoInterop.setMonarchTokensProvider();
        monacoInterop.setLanguageConfiguration();
        monacoInterop.CSharpRegister();
    } else if (elementId = 'resultId') {
        editor = module.createEditor(elementId, code);
    }
    monacoInterop.editors[elementId] = editor;
}


monacoInterop.CSharpRegister = (elementId) => {
    let languageId = "csharp";
    let autoRun = false;
    //TODO
    monaco.languages.registerSignatureHelpProvider(languageId, {
        signatureHelpTriggerCharacters: ["("],
        signatureHelpRetriggerCharacters: [","],
        provideSignatureHelp: (model, position, token, context) => {
        }
    });
}

//注册C#语言的语法提示、快捷键等
monacoInterop.registerMonacoProviders = async (dotNetObject) => {

    let autoRun = false;

    // 注册自动完成提供程序
    monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: ['.', ' ', ','],
        provideCompletionItems: async (model, position) => {
            const suggestions = await getProvidersAsync(model.getValue(), model.getOffsetAt(position));
            return { suggestions: suggestions };
        }
    });

    //注册悬浮提示
    monaco.languages.registerHoverProvider('csharp', {
        provideHover: async function (model, position) {
            const code = model.getValue();
            const cursor = model.getOffsetAt(position);
            const data = await dotNetObject.invokeMethodAsync('HoverInfoProvide', code, cursor);
            const result = JSON.parse(data);
            if (result) {
                const posStart = model.getPositionAt(result.OffsetFrom);
                const posEnd = model.getPositionAt(result.OffsetTo);
                const range = new monaco.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column);
                return {
                    range: range,
                    contents: [
                        { value: result.Information }
                    ]
                };
            }
        }
    });

    //监听变化
    onDidChangeContent();
    //调整两个editor的大小
    module.handleMouseMove();
    //添加快捷键
    addCommand();
    //添加右键菜单
    addAction();

    /**
     * 获取编辑器内容的语法错误
     * @param {monaco.editor.ITextModel} model - 编辑器的 Text Model
     */
    async function getModelMarkers(model) {
        let code = model.getValue();
        let result = await dotNetObject.invokeMethodAsync('GetModelMarkers', code, 80);
        let markers = [];
        let posStart;
        let posEnd;
        result = JSON.parse(result);
        for (let elem of result) {
            posStart = model.getPositionAt(elem.OffsetFrom);
            posEnd = model.getPositionAt(elem.OffsetTo);
            markers.push({
                severity: elem.Severity,
                startLineNumber: posStart.lineNumber,
                startColumn: posStart.column,
                endLineNumber: posEnd.lineNumber,
                endColumn: posEnd.column,
                message: elem.Message,
                code: elem.Id
            });
        }
        monaco.editor.setModelMarkers(model, 'csharp', markers);
    }

    /**
     * 当编辑器内容发生变化时，执行回调函数
     */
    async function onDidChangeContent() {
        csharpEditor.getModel().onDidChangeContent(async (event) => {
            getModelMarkers(csharpEditor.getModel());
            if (autoRun) {
                const position = csharpEditor.getPosition();
                const model = csharpEditor.getModel();
                const lineContent = model.getLineContent(position.lineNumber);
                const char = lineContent.charAt(position.column - 1);
                console.log(char + " text")
                if (char === ';') {
                    const result = await dotNetObject.invokeMethodAsync("AutoRunCode", csharpEditor.getValue());
                    monacoInterop.editors['resultId'].setValue(result);
                }
            }
        })
    }

    /**
     * 异步函数，用于从后端获取建议列表
     * @param {string} code - 当前编辑器中的代码
     * @param {monaco.Position} position - 光标所在位置
     * @returns {Array} - 建议列表
     */
    async function getProvidersAsync(code, position) {
        let suggestions = [];
        await dotNetObject.invokeMethodAsync('ProvideCompletionItems', code, position).then(result => {
            let res = JSON.parse(result);
            suggestions = module.suggestionsTab.slice();
            for (let key in res) {
                suggestions.push({
                    label: {
                        label: key,
                        description: res[key]
                    },
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: key
                });
            }
        });
        return suggestions;
    }
   
    async function addCommand() {
        // 添加 Ctrl/Cmd + S 快捷键命令，保存代码到本地存储
        csharpEditor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => {
            localStorage.setItem('oldCode', csharpEditor.getValue());
        });

        // 添加 Ctrl/Cmd + K 快捷键命令，使用 .NET 方法格式化代码
        csharpEditor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK, () => {
            dotNetObject.invokeMethodAsync('FormatCode', csharpEditor.getValue())
                .then(formatCode => { csharpEditor.setValue(formatCode); });
        });


        // 添加 Ctrl/Cmd + D 快捷键命令，复制当前行并插入新行
        csharpEditor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyD, () => {
            let lineNumber = csharpEditor.getPosition().lineNumber;
            let lineText = csharpEditor.getModel().getLineContent(lineNumber);
            csharpEditor.getModel().applyEdits([
                { range: new monaco.Range(lineNumber, 1, lineNumber, 1), text: lineText + '\n' }
            ]);
            csharpEditor.setPosition(new monaco.Position(lineNumber + 1, lineText.length + 1));
        });
    }
    async function addAction() {
        module.addAction(csharpEditor);

        csharpEditor.addAction({
            id: 'checkRun',
            label: '切换运行模式',
            contextMenuOrder: 0,
            contextMenuGroupId: "code",
            run: async function (editor) {
                autoRun = !autoRun;
                console.log(autoRun);
            }
        });
    }
   
}


//注册词法分析
monacoInterop.setMonarchTokensProvider = () => monaco.languages.setMonarchTokensProvider(languageId, {
    defaultToken: module.Monarch.defaultToken,
    tokenPostfix: module.Monarch.tokenPostfix,
    brackets: module.Monarch.brackets,
    keywords: module.Monarch.keywords,
    namespaceFollows: module.Monarch.namespaceFollows,
    parenFollows: module.Monarch.parenFollows,
    operators: module.Monarch.operators,
    symbols: module.Monarch.symbols,
    // escape sequences
    escapes: module.Monarch.escapes,
    // The main tokenizer for our languages
    tokenizer: module.Monarch.tokenizer,
})

// 注册语言配置
monacoInterop.setLanguageConfiguration = () => monaco.languages.setLanguageConfiguration('csharp', {
    comments: module.languageConfiguration.comments,
    brackets: module.languageConfiguration.brackets,
    autoClosingPairs: module.languageConfiguration.autoClosingPairs,
    surroundingPairs: module.languageConfiguration.surroundingPairs,
    indentationRules: module.languageConfiguration.indentationRules,
});

// 创建model触发
monacoInterop.onDidCreateModel = () => monaco.editor.onDidCreateModel(function (model) {
    module.validate(model)
})

// 监听变化
monacoInterop.onDidChangeContent = () => monaco.editor.onDidChangeContent(() => {
    monaco.editor.setModelMarkers(model, 'csharp', []);
    clearTimeout(handle);
    handle = setTimeout(() => module.validate(), 500);
})

//设置主题颜色
monacoInterop.setTheme = () => {
    monaco.editor.defineTheme("myTheme", {
        base: "vs-dark",
        inherit: true,
        minimap: false,
        rules: [
            { token: 'keyword', foreground: 'ab1f9e', fontStyle: 'bold' },
            { token: 'string', foreground: '2f810f' },
            { token: 'number', foreground: '2f810f' },
            { token: 'comment', foreground: '5e5e5e', fontStyle: 'italic' },
            { token: 'number.float', foreground: 'ab1f9e' },
        ],
        colors: {
            'editor.background': '#1e1e1e',
            'editor.foreground': '#d4d4d4',
            'editor.lineHighlightBackground': '#2d2d2d',
            'editorLineNumber.foreground': '#d4d4d4',
            'editor.selectionBackground': '#3e3e3e',
            'editor.wordHighlightBackground': '#303030',
            'editorCursor.foreground': '#d4d4d4',
        },
        encodedTokensColors: ['#ab1f9e', '#2f810f', '#b5cea8', '#5e5e5e', '#ab1f9e'],
    });
    monaco.editor.setTheme("myTheme");
}

monacoInterop.getCode = (elementId) => monacoInterop.editors[elementId].getValue();

monacoInterop.setCode = (elementId, code) => monacoInterop.editors[elementId].setValue(code);

//快速修复
monacoInterop.quickFix = () => {
    monaco.languages.registerCodeActionProvider
}

//代码分享
monacoInterop.copyText = module.copyText;

//

window.monacoInterop = monacoInterop;

console.log("end")
