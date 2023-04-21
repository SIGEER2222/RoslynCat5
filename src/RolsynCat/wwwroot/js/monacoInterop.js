
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
    console.log('source:' + code);
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
    monacoInterop.editors[elementId] = editor;
    console.log("初始化完毕")
}


monacoInterop.CSharpRegister = (elementId) => {
    let languageId = "csharp";
}

//注册C#语言的语法提示、快捷键等
monacoInterop.registerMonacoProviders = async (dotNetObject) => {

    //TODO
    console.log("ssssssssss")
    //monaco.editor.onDidCreateModel(model => {
    //    console.log("新模型已创建:", model.uri.toString());
    //    model.onDidChangeContent(() => {
    //    let handle = null;
    //        console.log("创建了模型")
    //        //清除错误信息
    //        monaco.editor.setModelMarkers(model, 'csharp', []);
    //        //清除等待
    //        clearTimeout(handle);
    //        handle = setTimeout(() => getModelMarkers(model), 500);
    //    });
    //    module.validate();
    //})


    //
    async function getProvidersAsync(code, position) {
        let suggestions = [];
        await dotNetObject.invokeMethodAsync('ProvideCompletionItems', code, position).then(result => {
            let r = JSON.parse(result);
            for (let key in r) {
                suggestions.push({
                    label: {
                        label: key,
                        description: r[key]
                    },
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: key
                });
            }
        });
        return suggestionsTab.concat(suggestions);
    }

    let suggestionsTab = [];

    //定义常用快捷补全
    suggestionsTab.push({
        label: {
            label: 'cw',
            description: '快捷键Console.WriteLine();'
        },
        insertText: 'Console.WriteLine()'
    })

    //预加载
    suggestionsTab = await getProvidersAsync(sourceCode, 100);

    //注册自动完成提供程序
    monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: ['.', ' ', ','],
        provideCompletionItems: async (model, position) => {
            let cursor = model.getOffsetAt(position);
            const suggestions = await getProvidersAsync(model.getValue(), cursor);
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
            console.log(111)
            // 获取当前位置的单词
            const word = model.getWordUntilPosition(position);
            if (!word) {
                return null;
            }

            // 判断当前单词是否为 Console.WriteLine
            if (word.word !== 'Console' || model.getValueInRange({ startLineNumber: position.lineNumber, startColumn: word.endColumn, endLineNumber: position.lineNumber, endColumn: position.column }).indexOf("WriteLine") === -1) {
                return null;
            }

            // 提取 Console.WriteLine 函数签名和参数信息
            const signature = {
                label: "Console.WriteLine(value: string): void",
                documentation: "Writes the specified string value, followed by the current line terminator, to the standard output stream.",
                parameters: [{
                    label: "value: string",
                    documentation: "The string value to write."
                }]
            };

            // 返回 Console.WriteLine 函数签名信息
            return {
                value: {
                    activeSignature: 0,
                    activeParameter: Math.min(context.argumentIndex, signature.parameters.length - 1),
                    signatures: [signature]
                },
                dispose: () => { }
            };
        }
    });


    async function getModelMarkers(model) {
        let code = model.getValue();
        let result = await dotNetObject.invokeMethodAsync('GetModelMarkers', code, 80);
        console.log(result);
        let markers = [];
        let posStart;
        let posEnd;
        result = JSON.parse(result);
        console.log(result);
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
        //const markers = monaco.editor.getModelMarkers({ resource: uri })
        console.log(markers)
        monaco.editor.setModelMarkers(model, 'csharp', markers);
        // markers是返回的错误信息数组，可赋值给需要判断语法错误的关键词，如this.coderErrors = markers
    }

    monacoInterop.editors['editorId'].getModel().onDidChangeContent(event => {
        //TODO设置一个定时器
        getModelMarkers(monacoInterop.editors['editorId'].getModel());
    })


    //添加快捷键
    monacoInterop.editors['editorId'].addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => {
        localStorage.setItem('oldCode', monacoInterop.editors['editorId'].getValue());
    });
    monacoInterop.editors['editorId'].addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyK, () => {
        dotNetObject.invokeMethodAsync('FormatCode', monacoInterop.editors['editorId'].getValue())
            .then(formatCode => { monacoInterop.editors['editorId'].setValue(formatCode); });
    });

    //添加右键
    monacoInterop.editors['editorId'].addAction({
        id: "formatCode",
        label: "格式化代码 ctrl + k",
        contextMenuOrder: 0,
        contextMenuGroupId: "code",
        run: function (editor) {
            dotNetObject.invokeMethodAsync('FormatCode', editor.getValue())
                .then(formatCode => { editor.setValue(formatCode); });
        }
    });
    monacoInterop.editors['editorId'].addAction({
        id: "clear",
        label: "清除",
        contextMenuOrder: 1,
        contextMenuGroupId: "code",
        run: function (editor) {
            editor.setValue(defaultCode);
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

monacoInterop.setMarkers = (elementId, markers) => {
    const editor = monacoInterop.editors[elementId];
    const model = editor.getModel();
    monaco.editor.setModelMarkers(model, null, markers);
};

//快速修复
monacoInterop.quickFix = () => {
    monaco.languages.registerCodeActionProvider
}


//代码分享
monacoInterop.copyText = (text) => {
    var modal = document.getElementById("myModal");
    var span = document.getElementsByClassName("close")[0];

    console.log(text)
    navigator.clipboard.writeText(text)
        .then(() => {
            modal.style.display = "block";
        })
        .catch(err => {
            alert('Failed to copy: ', err);
        });

    span.onclick = function () {
        modal.style.display = "none";
    }
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}
window.monacoInterop = monacoInterop;

console.log("end")
