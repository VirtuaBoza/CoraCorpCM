﻿const path = require('path'); // module built into node.js for pathing
const webpack = require('webpack'); // to access plugins built into webpack
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const OptimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');

const commonEntries = {
  vendor: './client/vendor.js',
  main: './client/main.js',
  'main-validation': './client/main-validation.js',
  'collection-create': './client/collection-create.js',
};

const commonOutputPath = path.resolve(__dirname, 'wwwroot/dist');

const commonProvidePlugin = new webpack.ProvidePlugin({
  $: 'jquery',
  jQuery: 'jquery',
  'window.jQuery': 'jquery',
  Popper: ['popper.js', 'default']
});

const commonModuleRules = [{
  test: /\.scss$/,
  use: [
    MiniCssExtractPlugin.loader,
    "css-loader",
    "sass-loader"
  ]
}];

module.exports = [
  {
    mode: 'development',
    entry: commonEntries,
    output: {
      path: commonOutputPath,
      filename: '[name].js'
    },
    plugins: [
      commonProvidePlugin,
      new MiniCssExtractPlugin({
        filename: "[name].css",
        chunkFilename: "[id].css"
      })
    ],
    module: {
      rules: commonModuleRules
    }
  },
  {
    mode: 'production',
    optimization: {
      minimizer: [
        new UglifyJsPlugin({
          cache: true,
          parallel: true,
          sourceMap: true
        }),
        new OptimizeCssAssetsPlugin({})
      ]
    },
    entry: commonEntries,
    output: {
      path: commonOutputPath,
      filename: '[name].min.js'
    },
    plugins: [
      commonProvidePlugin,
      new MiniCssExtractPlugin({
        filename: "[name].min.css",
        chunkFilename: "[id].min.css"
      })
    ],
    module: {
      rules: commonModuleRules
    }
  },
];
