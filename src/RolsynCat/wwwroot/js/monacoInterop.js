
import * as module from "./module.js";

let languageId = "csharp";
let monacoInterop = {};

monacoInterop.editors = {};
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
        monacoInterop.setMonarchTokensProvider();
        monacoInterop.setLanguageConfiguration();
        monacoInterop.CSharpRegister();
    } else if (elementId = 'resultId') {
        editor = module.createEditor(elementId, code);
    }
    window.editor2 = editor;
    monacoInterop.editors[elementId] = editor;
}


monacoInterop.CSharpRegister = (elementId) => {
    let languageId = "csharp";
}

//注册C#语言的语法提示、快捷键等
monacoInterop.registerMonacoProviders = async (dotNetObject) => {

    module.handleMouseMove();

    //注册自动完成提供程序
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

            const result = await dotNetObject.invokeMethodAsync('HoverInfoProvide', code, cursor);
            const r = JSON.parse(result);
            console.log(r)
            if (r) {
                let posStart = model.getPositionAt(r.OffsetFrom);
                let posEnd = model.getPositionAt(r.OffsetTo);

                return {
                    range: new monaco.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column),
                    contents: [
                        { value: r.Information }
                    ]
                };
            }
        }
    });

    //TODO
    monaco.languages.registerSignatureHelpProvider(languageId, {
        signatureHelpTriggerCharacters: ["("],
        signatureHelpRetriggerCharacters: [","],
        provideSignatureHelp: (model, position, token, context) => {
        }
    });


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
        console.log(markers)
        monaco.editor.setModelMarkers(model, 'csharp', markers);
    }

    monacoInterop.editors['editorId'].getModel().onDidChangeContent(event => {
        getModelMarkers(monacoInterop.editors['editorId'].getModel());
    })


    //添加快捷键
    let editor = monacoInterop.editors['editorId'];

    // 添加 Ctrl/Cmd + S 快捷键命令，保存代码到本地存储
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => {
        localStorage.setItem('oldCode', monacoInterop.editors['editorId'].getValue());
    });

    // 添加 Ctrl/Cmd + K 快捷键命令，使用 .NET 方法格式化代码
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK, () => {
        dotNetObject.invokeMethodAsync('FormatCode', monacoInterop.editors['editorId'].getValue())
            .then(formatCode => { monacoInterop.editors['editorId'].setValue(formatCode); });
    });
    editor.addCommand(monaco.KeyCode.F2, () => {
        console.log(111);
    });

    // 添加 Ctrl/Cmd + D 快捷键命令，复制当前行并插入新行
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyD, () => {
        let lineNumber = editor.getPosition().lineNumber;
        let lineText = editor.getModel().getLineContent(lineNumber);
        editor.getModel().applyEdits([
            { range: new monaco.Range(lineNumber, 1, lineNumber, 1), text: lineText + '\n' }
        ]);
        editor.setPosition(new monaco.Position(lineNumber + 1, lineText.length + 1));
    });

    //添加右键
    module.addAction(editor);
    let isChecked = false;

    let myAction = editor.addAction({
        id: 'checkRun',
        label: '切换成自动运行',
        contextMenuOrder: 0,
        contextMenuGroupId: "code",
        run: async function (editor) {
            editor.onDidChangeModelContent(async (event) => {
                const position = editor.getPosition();
                const range = new monaco.Range(position.lineNumber, position.column - 1, position.lineNumber, position.column);
                const text = editor.getModel().getValueInRange(range);
                if (text.slice(-1) === ';') {
                    console.log(text);
                    const result = await dotNetObject.invokeMethodAsync("AutoRunCode", editor.getValue());
                    console.log("result" + result);
                    monacoInterop.editors["resultId"].setValue(result);
                }
            });
        }
    });
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
