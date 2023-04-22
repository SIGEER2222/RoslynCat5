
let assemblies = null;



export function showMessageBox() {
    // 获取消息框元素
    const messageBox = document.querySelector('#messageBox');
    // 显示消息框
    messageBox.style.display = 'block';
    // 在 5 秒后自动关闭消息框
    setTimeout(function () {
        hideMessageBox();
    }, 5000);

    function hideMessageBox() {
        // 隐藏消息框
        messageBox.style.display = 'none';
    }
}

/**
 * 复制文本到剪贴板，并显示一个模态框。
 *
 * @param {string} text - 要复制的文本。
 */
export function copyText(text) {
    // 将文本复制到剪贴板
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log(text);
        })
        .catch(err => {
            // 复制失败时弹出警告框
            alert('Failed to copy: ', err);
        });
}

async function sendRequest(type, request) {
    let endPoint;
    const headers = {
        'Content-Type': 'application/json'
    };
    switch (type) {
        case 'complete': endPoint = '/completion/complete'; break;
        case 'signature': endPoint = '/completion/signature'; break;
        case 'hover': endPoint = '/completion/hover'; break;
        case 'codeCheck': endPoint = '/completion/codeCheck'; break;
    }
    // 添加请求拦截器
    axios.interceptors.request.use(request => {
        console.log('Starting Request', request)
        return request
    });

    const response = await axios.post(endPoint, request);
    console.log(response)
    return response.data;
}
export const languageId = 'csharp';

/**
 * 
 */
export const Monarch = {
    defaultToken: 'invalid',
    tokenPostfix: '.cs',

    brackets: [
        { open: '{', close: '}', token: 'delimiter.curly' },
        { open: '[', close: ']', token: 'delimiter.square' },
        { open: '(', close: ')', token: 'delimiter.parenthesis' },
        { open: '<', close: '>', token: 'delimiter.angle' }
    ],

    keywords: [
        'extern', 'alias', 'using', 'bool', 'decimal', 'sbyte', 'byte', 'short',
        'ushort', 'int', 'uint', 'long', 'ulong', 'char', 'float', 'double',
        'object', 'dynamic', 'string', 'assembly', 'is', 'as', 'ref',
        'out', 'this', 'base', 'new', 'typeof', 'void', 'checked', 'unchecked',
        'default', 'delegate', 'var', 'const', 'if', 'else', 'switch', 'case',
        'while', 'do', 'for', 'foreach', 'in', 'break', 'continue', 'goto',
        'return', 'throw', 'try', 'catch', 'finally', 'lock', 'yield', 'from',
        'let', 'where', 'join', 'on', 'equals', 'into', 'orderby', 'ascending',
        'descending', 'select', 'group', 'by', 'namespace', 'partial', 'class',
        'field', 'event', 'method', 'param', 'property', 'public', 'protected',
        'internal', 'private', 'abstract', 'sealed', 'static', 'struct', 'readonly',
        'volatile', 'virtual', 'override', 'params', 'get', 'set', 'add', 'remove',
        'operator', 'true', 'false', 'implicit', 'explicit', 'interface', 'enum',
        'null', 'async', 'await', 'fixed', 'sizeof', 'stackalloc', 'unsafe', 'nameof',
        'when'
    ],

    typeKeywords: [
        'boolean', 'double', 'byte', 'int', 'short', 'char', 'void', 'long', 'float'
    ],

    namespaceFollows: [
        'namespace', 'using',
    ],

    parenFollows: [
        'if', 'for', 'while', 'switch', 'foreach', 'using', 'catch', 'when'
    ],

    operators: [
        '=', '??', '||', '&&', '|', '^', '&', '==', '!=', '<=', '>=', '<<',
        '+', '-', '*', '/', '%', '!', '~', '++', '--', '+=',
        '-=', '*=', '/=', '%=', '&=', '|=', '^=', '<<=', '>>=', '>>', '=>'
    ],

    symbols: /[=><!~?:&|+\-*\/\^%]+/,

    // escape sequences
    escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

    // The main tokenizer for our languages
    tokenizer: {
        root: [

            // identifiers and keywords
            [/\@?[a-zA-Z_]\w*/, {
                cases: {
                    '@namespaceFollows': { token: 'keyword.$0', next: '@namespace' },
                    '@keywords': { token: 'keyword.$0', next: '@qualified' },
                    '@default': { token: 'identifier', next: '@qualified' }
                }
            }],

            // whitespace
            { include: '@whitespace' },

            // delimiters and operators
            [/}/, {
                cases: {
                    '$S2==interpolatedstring': { token: 'string.quote', next: '@pop' },
                    '$S2==litinterpstring': { token: 'string.quote', next: '@pop' },
                    '@default': '@brackets'
                }
            }],
            [/[{}()\[\]]/, '@brackets'],
            [/[<>](?!@symbols)/, '@brackets'],
            [/@symbols/, {
                cases: {
                    '@operators': 'delimiter',
                    '@default': ''
                }
            }],

            // numbers
            [/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, 'number.float'],
            [/0[xX][0-9a-fA-F_]+/, 'number.hex'],
            [/0[bB][01_]+/, 'number.hex'], // binary: use same theme style as hex
            [/[0-9_]+/, 'number'],

            // delimiter: after number because of .\d floats
            [/[;,.]/, 'delimiter'],

            // strings
            [/"([^"\\]|\\.)*$/, 'string.invalid'],  // non-teminated string
            [/"/, { token: 'string.quote', next: '@string' }],
            [/\$\@"/, { token: 'string.quote', next: '@litinterpstring' }],
            [/\@"/, { token: 'string.quote', next: '@litstring' }],
            [/\$"/, { token: 'string.quote', next: '@interpolatedstring' }],

            // characters
            [/'[^\\']'/, 'string'],
            [/(')(@escapes)(')/, ['string', 'string.escape', 'string']],
            [/'/, 'string.invalid']
        ],

        qualified: [
            [/[a-zA-Z_][\w]*/, {
                cases: {
                    '@keywords': { token: 'keyword.$0' },
                    '@default': 'identifier'
                }
            }],
            [/\./, 'delimiter'],
            ['', '', '@pop'],
        ],

        namespace: [
            { include: '@whitespace' },
            [/[A-Z]\w*/, 'namespace'],
            [/[\.=]/, 'delimiter'],
            ['', '', '@pop'],
        ],

        comment: [
            [/[^\/*]+/, 'comment'],
            // [/\/\*/,    'comment', '@push' ],    // no nested comments :-(
            ['\\*/', 'comment', '@pop'],
            [/[\/*]/, 'comment']
        ],

        string: [
            [/[^\\"]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litstring: [
            [/[^"]+/, 'string'],
            [/""/, 'string.escape'],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        litinterpstring: [
            [/[^"{]+/, 'string'],
            [/""/, 'string.escape'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.litinterpstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        interpolatedstring: [
            [/[^\\"{]+/, 'string'],
            [/@escapes/, 'string.escape'],
            [/\\./, 'string.escape.invalid'],
            [/{{/, 'string.escape'],
            [/}}/, 'string.escape'],
            [/{/, { token: 'string.quote', next: 'root.interpolatedstring' }],
            [/"/, { token: 'string.quote', next: '@pop' }]
        ],

        whitespace: [
            [/^[ \t\v\f]*#((r)|(load))(?=\s)/, 'directive.csx'],
            [/^[ \t\v\f]*#\w.*$/, 'namespace.cpp'],
            [/[ \t\v\f\r\n]+/, ''],
            [/\/\*/, 'comment', '@comment'],
            [/\/\/.*$/, 'comment'],
        ],
    },
};

export const languageConfiguration = {

    comments: {
        lineComment: '//',
        blockComment: ['/*', '*/'],
    },
    brackets: [
        ['{', '}'],
        ['[', ']'],
        ['(', ')']
    ],
    autoClosingPairs: [
        { open: '{', close: '}' },
        { open: '[', close: ']' },
        { open: '(', close: ')' },
        { open: '\'', close: '\'', notIn: ['string', 'comment'] },
        { open: '\"', close: '\"', notIn: ['string'] }
    ],
    surroundingPairs: [
        { open: '{', close: '}' },
        { open: '[', close: ']' },
        { open: '(', close: ')' },
        { open: '\'', close: '\'' },
        { open: '\"', close: '\"' }
    ],
    indentationRules: {
        decreaseIndentPattern: /^((?!.*?\/\*).*\*\/)?\s*[\}\]\)].*$/,
        increaseIndentPattern: /^((?!\/\/).)*(\{[^}"'`]*|\([^)"'`]*|\[[^\]"'`]*)$/
    }
}

export const legend = {
    tokenTypes: [
        "comment",
        "string",
        "keyword",
        "number",
        "regexp",
        "operator",
        "namespace",
        "type",
        "struct",
        "class",
        "interface",
        "enum",
        "typeParameter",
        "function",
        "member",
        "macro",
        "variable",
        "parameter",
        "property",
        "label",
    ],
    tokenModifiers: [
        "declaration",
        "documentation",
        "readonly",
        "static",
        "abstract",
        "deprecated",
        "modification",
        "async",
    ],
}

const CSHARP_TOKENS = {
    CLASS: "class",
    METHOD: "method",
    PROPERTY: "property",
    COMMENT: "comment",
    STRING: "string",
    NUMBER: "number",
    KEYWORD: "keyword",
    IDENTIFIER: "identifier",
};
const CSHARP_LEGEND = {
    tokenTypes: [
        CSHARP_TOKENS.CLASS,
        CSHARP_TOKENS.METHOD,
        CSHARP_TOKENS.PROPERTY,
        CSHARP_TOKENS.COMMENT,
        CSHARP_TOKENS.STRING,
        CSHARP_TOKENS.NUMBER,
        CSHARP_TOKENS.KEYWORD,
        CSHARP_TOKENS.IDENTIFIER,
    ],
    tokenModifiers: ["static", "readonly", "async"],
    colors: {
        [CSHARP_TOKENS.CLASS]: "#569cd6",
        [CSHARP_TOKENS.METHOD]: "#c586c0",
        [CSHARP_TOKENS.PROPERTY]: "#9cdcfe",
        [CSHARP_TOKENS.COMMENT]: "#6a9955",
        [CSHARP_TOKENS.STRING]: "#ce9178",
        [CSHARP_TOKENS.NUMBER]: "#b5cea8",
        [CSHARP_TOKENS.KEYWORD]: "#4ec9b0",
        [CSHARP_TOKENS.IDENTIFIER]: "#dcdcaa",
    },
};
const CSHARP_TOKEN_PATTERN = /(?<=\W|^)(class|void|int|string|bool|true|false|null|var|new|async|await|namespace|using)(?=\W|$)|(?<=\W|^)(static|readonly)(\.[A-Za-z0-9_]+)*(?=\W|$)|(?<=\W|^)(\w+)(?= *\()|\/\/.*|\/\*[\s\S]*?\*\//gm;


export function getType(type) {
    return legend.tokenTypes.indexOf(type);
}

/** 获取mod
 *  @type {(modifier: string[]|string|null)=>number} 
 * @param grid {Ext.Grid.Panel} 需要合并的Grid
* @param cols {Array} 需要合并列的Index(序号)数组；从0开始计数，序号也包含。
* @param isAllSome {Boolean} ：是否2个tr的cols必须完成一样才能进行合并。true：完成一样；false(默认)：不完全一样
* @return void
* @author polk6 2015/07/21 
* @example
* _________________                             _________________
* |  年龄 |  姓名 |                             |  年龄 |  姓名 |
* -----------------      mergeCells(grid,[0])   -----------------
* |  18   |  张三 |              =>             |       |  张三 |
* -----------------                             -  18   ---------
* |  18   |  王五 |                             |       |  王五 |
* -----------------                             -----------------
 */
export function getModifier(modifiers) {
    if (typeof modifiers === "string") {
        modifiers = [modifiers];
    }
    if (Array.isArray(modifiers)) {
        let nModifiers = 0;
        for (let modifier of modifiers) {
            const nModifier = legend.tokenModifiers.indexOf(modifier);
            if (nModifier > -1) {
                nModifiers |= (1 << nModifier) >>> 0;
            }
        }
        return nModifiers;
    } else {
        return 0;
    }
}
export function registerDocumentSemanticTokensProvider() {
    monaco.languages.registerDocumentSemanticTokensProvider(languageId, {
        getLegend: function () { return CSHARP_LEGEND; },
        provideDocumentSemanticTokens: function (model, lastResultId, token) {

            if (lastResultId) {
                let cachedResult = cache.get(lastResultId);
                if (cachedResult) {
                    return cachedResult;
                }
            }

            var lines = model.getLinesContent();
            var data = [];

            let result = computeDocumentSemanticTokens(model, token);

            // 将结果添加到缓存中
            let resultId = uuidv4();
            cache.set(resultId, result);

            return {
                data: result.tokens,
                resultId,
            };
        },
        releaseDocumentSemanticTokens: function (resultId) {
            // Release any resources associated with the given resultId here...
            cache.delete(resultId);
        }
    });

    function computeDocumentSemanticTokens(model, token) {
        let lines = model.getLinesContent();

        let tokens = [];
        let offset = 0;

        for (let i = 0; i < lines.length; i++) {
            let line = lines[i];

            let lineTokens = computeLineSemanticTokens(line, offset, token);
            tokens = tokens.concat(lineTokens);
            offset += line.length + 1; // +1 是因为每行末尾会有一个换行符
        }

        return { tokens };
    }

    function computeLineSemanticTokens(line, offset, token) {
        let tokens = [];

        // ... 根据语法规则计算出该行的语义单元 ...

        return tokens;
    }

    let cache = new Map();
}

export function createEditor(elementId, value) {
    let editor = monaco.editor.create(document.getElementById(elementId), {
        value: value,
        language: languageId,
        theme: "vs-dark",
        tabSize: 4,
        insertSpaces: false,
        wrappingIndent: "indent",
        wordWrap: "wordWrapColumn",
        "semanticHighlighting.enabled": true,
        minimap: {
            enabled: false
        },
        formatOnType: true,
        formatOnPaste: true,
        autoIndent: true,
        fontSize: 16,
        lineHeight: 22,
        fontFamily: 'Consolas,Menlo,Monaco,monospace',
        automaticLayout: true,
        contextmenu: true,
        copyWithSyntaxHighlighting: true,
    });
    return editor;
}


/**
 * 提供用于代码自动补全的建议项数组
 *
 * @typedef {Object} CompletionLabel
 * @property {string} label - 建议项的显示文本
 * @property {string} description - 建议项的描述
 *
 * @typedef {Object} CompletionItem
 * @property {CompletionLabel} label - 建议项的标签
 * @property {monaco.languages.CompletionItemKind} kind - 建议项的种类
 * @property {monaco.languages.CompletionItemInsertTextRule} insertTextRules - 建议项的插入文本规则
 * @property {string} insertText - 建议项的插入文本
 * @property {string} sortText - 建议项的排序文本
 *
 * @type {CompletionItem[]} 建议项数组
 */
export const suggestionsTab = [
    {
        label: {
            label: 'cw',
            description: '快捷键Console.WriteLine();',
        },
        kind: monaco.languages.CompletionItemKind.Snippet,
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        insertText: 'Console.WriteLine(${1:msg});',
        sortText: '0',
    },
    {
        label: {
            label: 'for',
            description: 'for 循环',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        insertText: 'for (int i = 0; i < ${1:length}; i++)\n{\n\t${2://code...}\n}',
    },
    {
        label: {
            label: 'prop',
            description: '自动属性',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText: 'public ${1:type} ${2:PropertyName} { get; set; }',
    },
    {
        label: {
            label: 'while',
            description: 'while 循环',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText: 'while (${1:condition})\n{\n\t${2://code...}\n}',
    },
    {
        label: {
            label: 'try',
            description: 'try-catch 块',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText:
            'try\n{\n\t${1://code...}\n}\ncatch (${2:Exception} ex)\n{\n\t${3://handling code...}\n}',
    },
    {
        label: {
            label: 'foreach',
            description: 'foreach 循环',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText:
            'foreach (${1:type} ${2:item} in ${3:collection})\n{\n\t${4://code...}\n\t${5:Console.WriteLine(${2});}\n}',
    },
    {
        label: {
            label: 'if',
            description: 'if 语句',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText: 'if (${1:condition}) {\n\t${2://code...}\n}$0',
    },
    {
        label: {
            label: 'else',
            description: 'else 语句',
        },
        insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
        sortText: '0',
        insertText: 'else {\n\t${1://code...}\n}$0',
    },
];
