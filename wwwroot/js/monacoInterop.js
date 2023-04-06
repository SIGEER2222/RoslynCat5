
import * as module from "./module.js";

let languageId = "csharp";
let monacoInterop = {};
monacoInterop.editors = {};

//创建和初始化编辑器
monacoInterop.createEditor = (elementId, code) => {

    console.log("开始初始化")
    let editor = monaco.editor.create(document.getElementById(elementId), {
        value: code,
        language: languageId,
        theme: "vs-dark",
        wrappingIndent: "indent",
        wordWrap: "wordWrapColumn",
        "semanticHighlighting.enabled": true,
        minimap: {
            enabled: false
        },
        fontSize: 16,
        lineHeight: 22,
        fontFamily: 'Consolas,Menlo,Monaco,monospace',
        automaticLayout: true,
        contextmenu: true,
        copyWithSyntaxHighlighting: true,
    });
    monacoInterop.editors[elementId] = editor;

    monacoInterop.setMonarchTokensProvider();
    monacoInterop.setLanguageConfiguration();
    monacoInterop.CSharpRegister();

    console.log("初始化完毕")
}

monacoInterop.CSharpRegister = (elementId) => {
    let languageId = "csharp";

    monaco.editor.onDidCreateModel(function (model) {
        var handle = null;
        model.onDidChangeContent(() => {
            console.log("ondi")
            monaco.editor.setModelMarkers(model, 'csharp', []);
            clearTimeout(handle);
            handle = setTimeout(() => module.validate(),500);
        });
        module.validate();
    })
    monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: [".", " "],
        provideCompletionItems: module.provideCompletionItems
    });
    monaco.languages.registerSignatureHelpProvider(languageId, {
        signatureHelpTriggerCharacters: ["("],
        signatureHelpRetriggerCharacters: [","],
        provideSignatureHelp: module.provideSignatureHelp
    });
    monaco.languages.registerHoverProvider(languageId, {
        provideHover: module.provideHover
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

// 注册自动完成提供程序
monacoInterop.registerCompletionItemProvider = () => monaco.languages.registerCompletionItemProvider(languageId, {
    triggerCharacters: [".", " "],
    provideCompletionItems: module.provideCompletionItems,
})


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

monacoInterop.registerDocumentSemanticTokensProvider = () =>
    monaco.languages.registerDocumentSemanticTokensProvider(languageId, {
        getLegend: function () { return legend; },
        provideDocumentSemanticTokens: () => { },
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