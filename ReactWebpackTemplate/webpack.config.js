const path = require('path'),
    webpack = require('webpack');

module.exports = {
    stats: {
        colors: true,
        timings: true
    },
    entry: {
        'vendor': [
            //'axios',
            'react-transition-group',
            'react-bootstrap',
            path.resolve(path.join(__dirname, 'node_modules/bootstrap/dist/css/bootstrap.css')),
            'fontawesome'
        ],
        'app': [
            './Client/index',
            './Content/base.css',
            //'webpack-hot-middleware/client'
            //'webpack-dev-server/client?http://0.0.0.0:3009',
            'webpack/hot/only-dev-server'
        ]
    },

    output: {
        path: path.join(__dirname, 'build'),
        publicPath: '/dist/',
        sourceMapFilename: '[name].js.map',
        filename: '[name].js'
    },

    devtool: 'source-map',

    plugins: [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NoEmitOnErrorsPlugin()
    ],

    module: {
        loaders: [
            {
                test: /\.js$/,
                include: /Client/,
                //exclude: /node_modules/,
                loader: 'babel-loader',
                query: { presets: ['react', 'es2015', 'stage-2', 'flow'] }
            },
            { test: /\.css$/, loader: 'style-loader!css-loader' },
            { test: /\.(png|jpg|gif|ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'file-loader' },
            { test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'url-loader?limit=10000&mimetype=application/font-woff' },
        ]
    }
};